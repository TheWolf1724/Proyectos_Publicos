using Microsoft.Extensions.Logging;
using PortMonitor.Core.Interfaces;
using PortMonitor.Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace PortMonitor.Core.Services
{
    /// <summary>
    /// Implementación del monitor de puertos para Windows
    /// </summary>
    public class WindowsPortMonitor : IPortMonitor
    {
        private readonly ILogger<WindowsPortMonitor> _logger;
        private readonly AppConfiguration _configuration;
        private readonly Dictionary<string, PortEvent> _lastKnownPorts;
        private Timer? _monitoringTimer;
        private bool _isMonitoring;

        public bool IsMonitoring => _isMonitoring;
        public event Action<PortEvent>? PortChanged;

        public WindowsPortMonitor(ILogger<WindowsPortMonitor> logger, AppConfiguration configuration)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _lastKnownPorts = new Dictionary<string, PortEvent>();
        }

        public Task StartMonitoringAsync()
        {
            if (_isMonitoring)
            {
                _logger.LogWarning("Port monitoring is already running");
                return Task.CompletedTask;
            }

            _logger.LogInformation("Starting port monitoring");
            
            // Obtener estado inicial
            var initialPorts = GetCurrentPortsInternal();
            foreach (var port in initialPorts)
            {
                var key = GetPortKey(port);
                _lastKnownPorts[key] = port;
            }

            // Iniciar timer de monitoreo
            _monitoringTimer = new Timer(async _ => await MonitorPortsAsync(), 
                null, TimeSpan.Zero, TimeSpan.FromMilliseconds(_configuration.MonitoringIntervalMs));
            
            _isMonitoring = true;
            _logger.LogInformation("Port monitoring started successfully");
            
            return Task.CompletedTask;
        }

        public Task StopMonitoringAsync()
        {
            if (!_isMonitoring)
            {
                _logger.LogWarning("Port monitoring is not running");
                return Task.CompletedTask;
            }

            _logger.LogInformation("Stopping port monitoring");
            
            _monitoringTimer?.Dispose();
            _monitoringTimer = null;
            _isMonitoring = false;
            
            _logger.LogInformation("Port monitoring stopped");
            return Task.CompletedTask;
        }

        public async Task<IEnumerable<PortEvent>> GetCurrentPortStatusAsync()
        {
            return await Task.Run(() => GetCurrentPortsInternal());
        }

        private async Task MonitorPortsAsync()
        {
            try
            {
                var currentPorts = await Task.Run(() => GetCurrentPortsInternal());
                var currentPortKeys = new HashSet<string>();

                // Verificar puertos nuevos o modificados
                foreach (var currentPort in currentPorts)
                {
                    var key = GetPortKey(currentPort);
                    currentPortKeys.Add(key);

                    if (!_lastKnownPorts.ContainsKey(key))
                    {
                        // Puerto nuevo detectado
                        currentPort.EventType = PortEventType.Opened;
                        currentPort.Timestamp = DateTime.Now;
                        _lastKnownPorts[key] = currentPort;
                        
                        _logger.LogDebug("New port detected: {ProcessName} ({ProcessId}) opened {Protocol} port {LocalPort}", 
                            currentPort.ProcessName, currentPort.ProcessId, currentPort.Protocol, currentPort.LocalPort);
                        
                        PortChanged?.Invoke(currentPort);
                    }
                    else
                    {
                        // Actualizar información del puerto existente
                        _lastKnownPorts[key] = currentPort;
                    }
                }

                // Verificar puertos cerrados
                var closedPorts = _lastKnownPorts.Keys.Except(currentPortKeys).ToList();
                foreach (var closedPortKey in closedPorts)
                {
                    var closedPort = _lastKnownPorts[closedPortKey];
                    closedPort.EventType = PortEventType.Closed;
                    closedPort.Timestamp = DateTime.Now;
                    
                    _logger.LogDebug("Port closed: {ProcessName} ({ProcessId}) closed {Protocol} port {LocalPort}", 
                        closedPort.ProcessName, closedPort.ProcessId, closedPort.Protocol, closedPort.LocalPort);
                    
                    PortChanged?.Invoke(closedPort);
                    _lastKnownPorts.Remove(closedPortKey);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during port monitoring");
            }
        }

        private List<PortEvent> GetCurrentPortsInternal()
        {
            var portEvents = new List<PortEvent>();

            try
            {
                if (_configuration.MonitorTcpPorts)
                {
                    portEvents.AddRange(GetTcpConnections());
                }

                if (_configuration.MonitorUdpPorts)
                {
                    portEvents.AddRange(GetUdpConnections());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current port status");
            }

            return portEvents;
        }

        private IEnumerable<PortEvent> GetTcpConnections()
        {
            var connections = new List<PortEvent>();
            
            try
            {
                var tcpConnections = IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpConnections();
                var tcpListeners = IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners();

                // Procesar conexiones TCP activas
                foreach (var connection in tcpConnections)
                {
                    var processInfo = GetProcessInfoForPort(connection.LocalEndPoint.Port, ProtocolType.TCP);
                    if (processInfo != null)
                    {
                        connections.Add(new PortEvent
                        {
                            ProcessId = processInfo.ProcessId,
                            ProcessName = processInfo.ProcessName,
                            ExecutablePath = processInfo.ExecutablePath,
                            UserName = processInfo.UserName,
                            LocalIpAddress = connection.LocalEndPoint.Address,
                            LocalPort = connection.LocalEndPoint.Port,
                            RemoteIpAddress = connection.RemoteEndPoint.Address,
                            RemotePort = connection.RemoteEndPoint.Port,
                            Protocol = ProtocolType.TCP,
                            Status = ConvertTcpState(connection.State),
                            EventType = PortEventType.Opened,
                            Timestamp = DateTime.Now
                        });
                    }
                }

                // Procesar listeners TCP
                foreach (var listener in tcpListeners)
                {
                    var processInfo = GetProcessInfoForPort(listener.Port, ProtocolType.TCP);
                    if (processInfo != null)
                    {
                        connections.Add(new PortEvent
                        {
                            ProcessId = processInfo.ProcessId,
                            ProcessName = processInfo.ProcessName,
                            ExecutablePath = processInfo.ExecutablePath,
                            UserName = processInfo.UserName,
                            LocalIpAddress = listener.Address,
                            LocalPort = listener.Port,
                            Protocol = ProtocolType.TCP,
                            Status = PortStatus.Listening,
                            EventType = PortEventType.Opened,
                            Timestamp = DateTime.Now
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting TCP connections");
            }

            return connections;
        }

        private IEnumerable<PortEvent> GetUdpConnections()
        {
            var connections = new List<PortEvent>();
            
            try
            {
                var udpListeners = IPGlobalProperties.GetIPGlobalProperties().GetActiveUdpListeners();

                foreach (var listener in udpListeners)
                {
                    var processInfo = GetProcessInfoForPort(listener.Port, ProtocolType.UDP);
                    if (processInfo != null)
                    {
                        connections.Add(new PortEvent
                        {
                            ProcessId = processInfo.ProcessId,
                            ProcessName = processInfo.ProcessName,
                            ExecutablePath = processInfo.ExecutablePath,
                            UserName = processInfo.UserName,
                            LocalIpAddress = listener.Address,
                            LocalPort = listener.Port,
                            Protocol = ProtocolType.UDP,
                            Status = PortStatus.Listening,
                            EventType = PortEventType.Opened,
                            Timestamp = DateTime.Now
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting UDP connections");
            }

            return connections;
        }

        private ProcessInfo? GetProcessInfoForPort(int port, ProtocolType protocol)
        {
            try
            {
                // Usar netstat para obtener la asociación puerto-proceso
                var processInfo = new ProcessStartInfo
                {
                    FileName = "netstat",
                    Arguments = "-ano",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };

                using var process = Process.Start(processInfo);
                if (process == null) return null;

                var output = process.StandardOutput.ReadToEnd();
                var lines = output.Split('\n');

                var protocolStr = protocol.ToString();
                foreach (var line in lines)
                {
                    if (line.Contains(protocolStr) && line.Contains($":{port} "))
                    {
                        var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length >= 5 && int.TryParse(parts[^1], out var pid))
                        {
                            return GetProcessDetails(pid);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting process info for port {Port}", port);
            }

            return null;
        }

        private ProcessInfo? GetProcessDetails(int pid)
        {
            try
            {
                var process = Process.GetProcessById(pid);
                var userName = GetProcessOwner(pid);

                return new ProcessInfo
                {
                    ProcessId = pid,
                    ProcessName = process.ProcessName,
                    ExecutablePath = GetProcessExecutablePath(process),
                    UserName = userName ?? "Unknown"
                };
            }
            catch
            {
                return null;
            }
        }

        private string GetProcessExecutablePath(Process process)
        {
            try
            {
                return process.MainModule?.FileName ?? "Unknown";
            }
            catch
            {
                try
                {
                    // Método alternativo usando WMI
                    using var searcher = new ManagementObjectSearcher(
                        $"SELECT ExecutablePath FROM Win32_Process WHERE ProcessId = {process.Id}");
                    using var results = searcher.Get();
                    
                    foreach (ManagementObject result in results)
                    {
                        return result["ExecutablePath"]?.ToString() ?? "Unknown";
                    }
                }
                catch
                {
                    // Ignorar errores y devolver valor por defecto
                }

                return "Unknown";
            }
        }

        private string? GetProcessOwner(int pid)
        {
            try
            {
                using var searcher = new ManagementObjectSearcher(
                    $"SELECT * FROM Win32_Process WHERE ProcessId = {pid}");
                using var results = searcher.Get();
                
                foreach (ManagementObject result in results)
                {
                    var owner = new string[2];
                    result.InvokeMethod("GetOwner", owner);
                    return $"{owner[1]}\\{owner[0]}";
                }
            }
            catch
            {
                // Ignorar errores
            }

            return null;
        }

        private static PortStatus ConvertTcpState(TcpState state)
        {
            return state switch
            {
                TcpState.Listen => PortStatus.Listening,
                TcpState.Established => PortStatus.Established,
                TcpState.TimeWait => PortStatus.TimeWait,
                TcpState.CloseWait => PortStatus.CloseWait,
                TcpState.FinWait1 => PortStatus.FinWait1,
                TcpState.FinWait2 => PortStatus.FinWait2,
                TcpState.SynSent => PortStatus.SynSent,
                TcpState.SynReceived => PortStatus.SynReceived,
                TcpState.Closed => PortStatus.Closed,
                _ => PortStatus.Closed
            };
        }

        private static string GetPortKey(PortEvent portEvent)
        {
            return $"{portEvent.ProcessId}_{portEvent.LocalPort}_{portEvent.Protocol}_{portEvent.LocalIpAddress}";
        }

        private class ProcessInfo
        {
            public int ProcessId { get; set; }
            public string ProcessName { get; set; } = string.Empty;
            public string ExecutablePath { get; set; } = string.Empty;
            public string UserName { get; set; } = string.Empty;
        }

        public void Dispose()
        {
            _monitoringTimer?.Dispose();
        }
    }
}
