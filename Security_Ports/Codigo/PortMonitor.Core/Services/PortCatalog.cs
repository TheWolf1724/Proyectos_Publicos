using Microsoft.Extensions.Logging;
using PortMonitor.Core.Interfaces;
using PortMonitor.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PortMonitor.Core.Services
{
    /// <summary>
    /// Catálogo de información de puertos con base de datos de conocimiento de seguridad
    /// </summary>
    public class PortCatalog : IPortCatalog
    {
        private readonly ILogger<PortCatalog> _logger;
        private readonly IDataRepository _dataRepository;
        private readonly Dictionary<string, PortInfo> _portDatabase;
        private readonly Dictionary<string, string[]> _malwareSignatures;

        public PortCatalog(ILogger<PortCatalog> logger, IDataRepository dataRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dataRepository = dataRepository ?? throw new ArgumentNullException(nameof(dataRepository));
            _portDatabase = new Dictionary<string, PortInfo>();
            _malwareSignatures = new Dictionary<string, string[]>();
            
            InitializePortDatabase();
            InitializeMalwareSignatures();
        }

        public async Task<PortInfo?> GetPortInfoAsync(int port, ProtocolType protocol)
        {
            try
            {
                var key = GetPortKey(port, protocol);
                
                // Buscar primero en la base de datos local
                if (_portDatabase.TryGetValue(key, out var portInfo))
                {
                    return portInfo;
                }

                // Buscar en el repositorio persistente
                var persistedInfo = await _dataRepository.GetPortInfoAsync(port, protocol);
                if (persistedInfo != null)
                {
                    _portDatabase[key] = persistedInfo;
                    return persistedInfo;
                }

                // Si no se encuentra, crear información básica
                return CreateUnknownPortInfo(port, protocol);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting port info for {Port}/{Protocol}", port, protocol);
                return null;
            }
        }

        public async Task<IEnumerable<PortInfo>> SearchPortsByServiceAsync(string serviceName)
        {
            try
            {
                return await Task.Run(() =>
                    _portDatabase.Values
                        .Where(p => p.ServiceName.Contains(serviceName, StringComparison.OrdinalIgnoreCase) ||
                                   p.Description.Contains(serviceName, StringComparison.OrdinalIgnoreCase))
                        .ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching ports by service: {ServiceName}", serviceName);
                return Enumerable.Empty<PortInfo>();
            }
        }

        public async Task<IEnumerable<PortInfo>> GetMaliciousPortsAsync()
        {
            try
            {
                return await Task.Run(() =>
                    _portDatabase.Values
                        .Where(p => p.RiskLevel >= RiskLevel.High || 
                                   p.Category == PortCategory.Malware ||
                                   (p.MalwareAssociations?.Length ?? 0) > 0)
                        .ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting malicious ports");
                return Enumerable.Empty<PortInfo>();
            }
        }

        public async Task<IEnumerable<PortInfo>> GetPortsByCategoryAsync(PortCategory category)
        {
            try
            {
                return await Task.Run(() =>
                    _portDatabase.Values
                        .Where(p => p.Category == category)
                        .ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting ports by category: {Category}", category);
                return Enumerable.Empty<PortInfo>();
            }
        }

        public async Task UpdatePortDatabaseAsync()
        {
            try
            {
                _logger.LogInformation("Updating port database");
                
                // En una implementación real, esto descargaría actualizaciones de una fuente externa
                // Por ahora, solo registramos que se intentó la actualización
                await Task.Delay(100);
                
                _logger.LogInformation("Port database update completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating port database");
            }
        }

        public async Task<RiskLevel> AnalyzePortRiskAsync(PortEvent portEvent)
        {
            try
            {
                var portInfo = await GetPortInfoAsync(portEvent.LocalPort, portEvent.Protocol);
                
                // Factores de riesgo
                var riskScore = 0;

                // Riesgo base del puerto
                if (portInfo != null)
                {
                    riskScore += portInfo.RiskLevel switch
                    {
                        RiskLevel.Low => 1,
                        RiskLevel.Medium => 3,
                        RiskLevel.High => 7,
                        RiskLevel.Critical => 10,
                        _ => 0
                    };

                    // Puerto asociado con malware
                    if (portInfo.MalwareAssociations?.Length > 0)
                    {
                        riskScore += 5;
                    }

                    // Categoría maliciosa
                    if (portInfo.Category == PortCategory.Malware)
                    {
                        riskScore += 8;
                    }
                }

                // Puertos no estándar (mayor a 1024 y no conocidos)
                if (portEvent.LocalPort > 1024 && (portInfo == null || !portInfo.IsWellKnown))
                {
                    riskScore += 2;
                }

                // Puertos muy altos (posible malware)
                if (portEvent.LocalPort > 49152)
                {
                    riskScore += 3;
                }

                // Proceso sospechoso (ubicación inusual)
                if (IsProcessPathSuspicious(portEvent.ExecutablePath))
                {
                    riskScore += 4;
                }

                // Usuario del sistema abriendo puertos
                if (IsSystemUser(portEvent.UserName) && portEvent.LocalPort > 1024)
                {
                    riskScore += 2;
                }

                // Patrones de malware conocidos
                if (await IsKnownMalwarePortAsync(portEvent.LocalPort, portEvent.Protocol))
                {
                    riskScore += 10;
                }

                // Determinar nivel de riesgo final
                return riskScore switch
                {
                    >= 15 => RiskLevel.Critical,
                    >= 10 => RiskLevel.High,
                    >= 5 => RiskLevel.Medium,
                    _ => RiskLevel.Low
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing port risk");
                return RiskLevel.Medium; // Por defecto si hay error
            }
        }

        public async Task<bool> IsKnownMalwarePortAsync(int port, ProtocolType protocol)
        {
            try
            {
                var key = GetPortKey(port, protocol);
                
                // Verificar en la base de datos de firmas de malware
                if (_malwareSignatures.ContainsKey(key))
                {
                    return true;
                }

                // Verificar en la información del puerto
                var portInfo = await GetPortInfoAsync(port, protocol);
                return portInfo?.MalwareAssociations?.Length > 0 || 
                       portInfo?.Category == PortCategory.Malware;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if port is known malware port");
                return false;
            }
        }

        private void InitializePortDatabase()
        {
            // Puertos de sistema y servicios conocidos
            AddPortInfo(new PortInfo { Port = 21, Protocol = ProtocolType.TCP, ServiceName = "FTP", Description = "File Transfer Protocol", RiskLevel = RiskLevel.Medium, Category = PortCategory.FileTransfer, IsWellKnown = true });
            AddPortInfo(new PortInfo { Port = 22, Protocol = ProtocolType.TCP, ServiceName = "SSH", Description = "Secure Shell", RiskLevel = RiskLevel.Medium, Category = PortCategory.RemoteAccess, IsWellKnown = true });
            AddPortInfo(new PortInfo { Port = 23, Protocol = ProtocolType.TCP, ServiceName = "Telnet", Description = "Telnet Protocol", RiskLevel = RiskLevel.High, Category = PortCategory.RemoteAccess, IsWellKnown = true, SecurityNotes = "Protocolo inseguro, transmite datos en texto plano" });
            AddPortInfo(new PortInfo { Port = 25, Protocol = ProtocolType.TCP, ServiceName = "SMTP", Description = "Simple Mail Transfer Protocol", RiskLevel = RiskLevel.Low, Category = PortCategory.Mail, IsWellKnown = true });
            AddPortInfo(new PortInfo { Port = 53, Protocol = ProtocolType.UDP, ServiceName = "DNS", Description = "Domain Name System", RiskLevel = RiskLevel.Low, Category = PortCategory.System, IsWellKnown = true });
            AddPortInfo(new PortInfo { Port = 80, Protocol = ProtocolType.TCP, ServiceName = "HTTP", Description = "HyperText Transfer Protocol", RiskLevel = RiskLevel.Low, Category = PortCategory.Web, IsWellKnown = true });
            AddPortInfo(new PortInfo { Port = 110, Protocol = ProtocolType.TCP, ServiceName = "POP3", Description = "Post Office Protocol v3", RiskLevel = RiskLevel.Low, Category = PortCategory.Mail, IsWellKnown = true });
            AddPortInfo(new PortInfo { Port = 143, Protocol = ProtocolType.TCP, ServiceName = "IMAP", Description = "Internet Message Access Protocol", RiskLevel = RiskLevel.Low, Category = PortCategory.Mail, IsWellKnown = true });
            AddPortInfo(new PortInfo { Port = 443, Protocol = ProtocolType.TCP, ServiceName = "HTTPS", Description = "HTTP Secure", RiskLevel = RiskLevel.Low, Category = PortCategory.Web, IsWellKnown = true });
            AddPortInfo(new PortInfo { Port = 993, Protocol = ProtocolType.TCP, ServiceName = "IMAPS", Description = "IMAP Secure", RiskLevel = RiskLevel.Low, Category = PortCategory.Mail, IsWellKnown = true });
            AddPortInfo(new PortInfo { Port = 995, Protocol = ProtocolType.TCP, ServiceName = "POP3S", Description = "POP3 Secure", RiskLevel = RiskLevel.Low, Category = PortCategory.Mail, IsWellKnown = true });

            // Puertos de bases de datos
            AddPortInfo(new PortInfo { Port = 1433, Protocol = ProtocolType.TCP, ServiceName = "MSSQL", Description = "Microsoft SQL Server", RiskLevel = RiskLevel.Medium, Category = PortCategory.Database, IsWellKnown = true });
            AddPortInfo(new PortInfo { Port = 3306, Protocol = ProtocolType.TCP, ServiceName = "MySQL", Description = "MySQL Database", RiskLevel = RiskLevel.Medium, Category = PortCategory.Database, IsWellKnown = true });
            AddPortInfo(new PortInfo { Port = 5432, Protocol = ProtocolType.TCP, ServiceName = "PostgreSQL", Description = "PostgreSQL Database", RiskLevel = RiskLevel.Medium, Category = PortCategory.Database, IsWellKnown = true });

            // Puertos de acceso remoto
            AddPortInfo(new PortInfo { Port = 3389, Protocol = ProtocolType.TCP, ServiceName = "RDP", Description = "Remote Desktop Protocol", RiskLevel = RiskLevel.High, Category = PortCategory.RemoteAccess, IsWellKnown = true, SecurityNotes = "Puerto común para ataques de fuerza bruta" });
            AddPortInfo(new PortInfo { Port = 5900, Protocol = ProtocolType.TCP, ServiceName = "VNC", Description = "Virtual Network Computing", RiskLevel = RiskLevel.High, Category = PortCategory.RemoteAccess, IsWellKnown = true });

            // Puertos conocidos de malware
            AddPortInfo(new PortInfo { Port = 1234, Protocol = ProtocolType.TCP, ServiceName = "Unknown", Description = "Puerto comúnmente usado por malware", RiskLevel = RiskLevel.Critical, Category = PortCategory.Malware, IsWellKnown = false, MalwareAssociations = new[] { "SubSeven", "Ultors Trojan" } });
            AddPortInfo(new PortInfo { Port = 4444, Protocol = ProtocolType.TCP, ServiceName = "Unknown", Description = "Puerto común de backdoors", RiskLevel = RiskLevel.Critical, Category = PortCategory.Malware, IsWellKnown = false, MalwareAssociations = new[] { "MetaSploit", "Various Trojans" } });
            AddPortInfo(new PortInfo { Port = 6666, Protocol = ProtocolType.TCP, ServiceName = "Unknown", Description = "Puerto usado por varios troyanos", RiskLevel = RiskLevel.Critical, Category = PortCategory.Malware, IsWellKnown = false, MalwareAssociations = new[] { "Back Construction", "NetBus" } });
            AddPortInfo(new PortInfo { Port = 12345, Protocol = ProtocolType.TCP, ServiceName = "Unknown", Description = "Puerto típico de NetBus trojan", RiskLevel = RiskLevel.Critical, Category = PortCategory.Malware, IsWellKnown = false, MalwareAssociations = new[] { "NetBus", "Whack-a-mole" } });

            _logger.LogInformation("Port database initialized with {Count} entries", _portDatabase.Count);
        }

        private void InitializeMalwareSignatures()
        {
            // Agregar firmas conocidas de malware
            _malwareSignatures["1234_TCP"] = new[] { "SubSeven", "Ultors Trojan" };
            _malwareSignatures["4444_TCP"] = new[] { "MetaSploit", "Various Trojans" };
            _malwareSignatures["6666_TCP"] = new[] { "Back Construction", "NetBus" };
            _malwareSignatures["12345_TCP"] = new[] { "NetBus", "Whack-a-mole" };
            _malwareSignatures["31337_TCP"] = new[] { "Back Orifice", "Elite hackers" };
            _malwareSignatures["54321_TCP"] = new[] { "Back Orifice 2000" };

            _logger.LogInformation("Malware signatures initialized with {Count} entries", _malwareSignatures.Count);
        }

        private void AddPortInfo(PortInfo portInfo)
        {
            var key = GetPortKey(portInfo.Port, portInfo.Protocol);
            _portDatabase[key] = portInfo;
        }

        private PortInfo CreateUnknownPortInfo(int port, ProtocolType protocol)
        {
            var riskLevel = RiskLevel.Low;
            var category = PortCategory.Unknown;

            // Puertos no estándar tienen mayor riesgo
            if (port > 1024)
            {
                riskLevel = RiskLevel.Medium;
            }

            // Puertos muy altos son más sospechosos
            if (port > 49152)
            {
                riskLevel = RiskLevel.High;
            }

            return new PortInfo
            {
                Port = port,
                Protocol = protocol,
                ServiceName = "Unknown Service",
                Description = $"Puerto {port} ({protocol}) - Servicio no identificado",
                RiskLevel = riskLevel,
                Category = category,
                IsWellKnown = false
            };
        }

        private bool IsProcessPathSuspicious(string executablePath)
        {
            if (string.IsNullOrEmpty(executablePath) || executablePath == "Unknown")
            {
                return true;
            }

            var suspiciousPaths = new[]
            {
                @"\Temp\",
                @"\AppData\Local\Temp\",
                @"\Users\Public\",
                @"\ProgramData\",
                @"\Windows\Temp\"
            };

            return suspiciousPaths.Any(path => 
                executablePath.Contains(path, StringComparison.OrdinalIgnoreCase));
        }

        private bool IsSystemUser(string userName)
        {
            if (string.IsNullOrEmpty(userName))
            {
                return false;
            }

            var systemUsers = new[]
            {
                "NT AUTHORITY\\SYSTEM",
                "NT AUTHORITY\\LOCAL SERVICE",
                "NT AUTHORITY\\NETWORK SERVICE",
                "SYSTEM"
            };

            return systemUsers.Any(user => 
                userName.Equals(user, StringComparison.OrdinalIgnoreCase));
        }

        private static string GetPortKey(int port, ProtocolType protocol)
        {
            return $"{port}_{protocol}";
        }
    }
}
