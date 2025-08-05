using Microsoft.Extensions.Logging;
using PortMonitor.Core.Interfaces;
using PortMonitor.Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace PortMonitor.Core.Services
{
    /// <summary>
    /// Implementación del administrador de firewall para Windows
    /// </summary>
    public class WindowsFirewallManager : IFirewallManager
    {
        private readonly ILogger<WindowsFirewallManager> _logger;
        private readonly IDataRepository _dataRepository;
        private const string RULE_PREFIX = "PortMonitor_";

        public WindowsFirewallManager(ILogger<WindowsFirewallManager> logger, IDataRepository dataRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dataRepository = dataRepository ?? throw new ArgumentNullException(nameof(dataRepository));
        }

        public async Task<bool> CreateRuleAsync(FirewallRule rule)
        {
            try
            {
                // Generar un nombre único para la regla
                rule.Name = $"{RULE_PREFIX}{rule.Id}_{DateTime.Now:yyyyMMddHHmmss}";
                
                // Construir el comando netsh
                var command = BuildNetshCommand(rule, "add");
                
                _logger.LogInformation("Creating firewall rule: {RuleName}", rule.Name);
                
                var success = await ExecuteNetshCommandAsync(command);
                
                if (success)
                {
                    rule.CreatedDate = DateTime.Now;
                    await _dataRepository.SaveFirewallRuleAsync(rule);
                    _logger.LogInformation("Firewall rule created successfully: {RuleName}", rule.Name);
                }
                else
                {
                    _logger.LogError("Failed to create firewall rule: {RuleName}", rule.Name);
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating firewall rule");
                return false;
            }
        }

        public async Task<bool> DeleteRuleAsync(int ruleId)
        {
            try
            {
                var rule = await _dataRepository.GetFirewallRuleAsync(ruleId);
                if (rule == null)
                {
                    _logger.LogWarning("Firewall rule not found: {RuleId}", ruleId);
                    return false;
                }

                var command = $"advfirewall firewall delete rule name=\"{rule.Name}\"";
                
                _logger.LogInformation("Deleting firewall rule: {RuleName}", rule.Name);
                
                var success = await ExecuteNetshCommandAsync(command);
                
                if (success)
                {
                    await _dataRepository.DeleteFirewallRuleAsync(ruleId);
                    _logger.LogInformation("Firewall rule deleted successfully: {RuleName}", rule.Name);
                }
                else
                {
                    _logger.LogError("Failed to delete firewall rule: {RuleName}", rule.Name);
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting firewall rule {RuleId}", ruleId);
                return false;
            }
        }

        public async Task<bool> UpdateRuleAsync(FirewallRule rule)
        {
            try
            {
                // Para actualizar, primero eliminamos la regla existente y luego creamos una nueva
                var existingRule = await _dataRepository.GetFirewallRuleAsync(rule.Id);
                if (existingRule != null)
                {
                    await DeleteRuleAsync(rule.Id);
                }

                rule.ModifiedDate = DateTime.Now;
                return await CreateRuleAsync(rule);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating firewall rule {RuleId}", rule.Id);
                return false;
            }
        }

        public async Task<IEnumerable<FirewallRule>> GetApplicationRulesAsync()
        {
            try
            {
                return await _dataRepository.GetFirewallRulesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting application firewall rules");
                return Enumerable.Empty<FirewallRule>();
            }
        }

        public async Task<bool> SetRuleEnabledAsync(int ruleId, bool enabled)
        {
            try
            {
                var rule = await _dataRepository.GetFirewallRuleAsync(ruleId);
                if (rule == null)
                {
                    _logger.LogWarning("Firewall rule not found: {RuleId}", ruleId);
                    return false;
                }

                var command = $"advfirewall firewall set rule name=\"{rule.Name}\" new enable={(enabled ? "yes" : "no")}";
                
                _logger.LogInformation("{Action} firewall rule: {RuleName}", 
                    enabled ? "Enabling" : "Disabling", rule.Name);
                
                var success = await ExecuteNetshCommandAsync(command);
                
                if (success)
                {
                    rule.IsEnabled = enabled;
                    rule.ModifiedDate = DateTime.Now;
                    await _dataRepository.UpdateFirewallRuleAsync(rule);
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting firewall rule enabled state {RuleId}", ruleId);
                return false;
            }
        }

        public async Task<bool> AllowProcessPortAsync(string processPath, int port, ProtocolType protocol)
        {
            try
            {
                var rule = new FirewallRule
                {
                    Name = $"Allow_{System.IO.Path.GetFileNameWithoutExtension(processPath)}_{port}_{protocol}",
                    Description = $"Allow {System.IO.Path.GetFileName(processPath)} on {protocol} port {port}",
                    Action = RuleAction.Allow,
                    Direction = RuleDirection.Both,
                    Scope = RuleScope.ProcessAndPort,
                    ProcessPath = processPath,
                    LocalPort = port,
                    Protocol = protocol,
                    IsUserCreated = false,
                    CreatedDate = DateTime.Now
                };

                return await CreateRuleAsync(rule);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error allowing process port {ProcessPath}:{Port}", processPath, port);
                return false;
            }
        }

        public async Task<bool> BlockProcessPortAsync(string processPath, int port, ProtocolType protocol)
        {
            try
            {
                var rule = new FirewallRule
                {
                    Name = $"Block_{System.IO.Path.GetFileNameWithoutExtension(processPath)}_{port}_{protocol}",
                    Description = $"Block {System.IO.Path.GetFileName(processPath)} on {protocol} port {port}",
                    Action = RuleAction.Block,
                    Direction = RuleDirection.Both,
                    Scope = RuleScope.ProcessAndPort,
                    ProcessPath = processPath,
                    LocalPort = port,
                    Protocol = protocol,
                    IsUserCreated = false,
                    CreatedDate = DateTime.Now
                };

                return await CreateRuleAsync(rule);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error blocking process port {ProcessPath}:{Port}", processPath, port);
                return false;
            }
        }

        public async Task<bool> IsFirewallEnabledAsync()
        {
            try
            {
                var command = "advfirewall show currentprofile";
                var output = await ExecuteNetshCommandWithOutputAsync(command);
                
                return output?.Contains("State ON") == true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking firewall status");
                return false;
            }
        }

        public async Task<bool> RestoreDefaultRulesAsync()
        {
            try
            {
                _logger.LogInformation("Restoring default firewall rules");
                
                var rules = await GetApplicationRulesAsync();
                var success = true;

                foreach (var rule in rules)
                {
                    var deleteSuccess = await DeleteRuleAsync(rule.Id);
                    if (!deleteSuccess)
                    {
                        success = false;
                        _logger.LogWarning("Failed to delete rule during restore: {RuleName}", rule.Name);
                    }
                }

                if (success)
                {
                    await _dataRepository.DeleteAllUserCreatedRulesAsync();
                    _logger.LogInformation("Default firewall rules restored successfully");
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring default firewall rules");
                return false;
            }
        }

        private string BuildNetshCommand(FirewallRule rule, string action)
        {
            var command = $"advfirewall firewall {action} rule name=\"{rule.Name}\"";
            
            // Dirección
            if (rule.Direction != RuleDirection.Both)
            {
                command += $" dir={(rule.Direction == RuleDirection.Inbound ? "in" : "out")}";
            }
            
            // Acción
            command += $" action={(rule.Action == RuleAction.Allow ? "allow" : "block")}";
            
            // Protocolo
            if (rule.Protocol.HasValue)
            {
                command += $" protocol={rule.Protocol.Value.ToString().ToLower()}";
            }
            
            // Puerto local
            if (rule.LocalPort.HasValue)
            {
                command += $" localport={rule.LocalPort.Value}";
            }
            
            // Puerto remoto
            if (rule.RemotePort.HasValue)
            {
                command += $" remoteport={rule.RemotePort.Value}";
            }
            
            // Dirección IP local
            if (rule.LocalIpAddress != null)
            {
                command += $" localip={rule.LocalIpAddress}";
            }
            
            // Dirección IP remota
            if (rule.RemoteIpAddress != null)
            {
                command += $" remoteip={rule.RemoteIpAddress}";
            }
            
            // Rango de IP
            if (!string.IsNullOrEmpty(rule.IpRange))
            {
                command += $" remoteip={rule.IpRange}";
            }
            
            // Proceso
            if (!string.IsNullOrEmpty(rule.ProcessPath))
            {
                command += $" program=\"{rule.ProcessPath}\"";
            }
            
            // Estado
            command += $" enable={(rule.IsEnabled ? "yes" : "no")}";
            
            return command;
        }

        private async Task<bool> ExecuteNetshCommandAsync(string command)
        {
            try
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = "netsh",
                    Arguments = command,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    Verb = "runas" // Ejecutar como administrador
                };

                using var process = Process.Start(processInfo);
                if (process == null) return false;

                await process.WaitForExitAsync();
                
                var output = await process.StandardOutput.ReadToEndAsync();
                var error = await process.StandardError.ReadToEndAsync();

                if (process.ExitCode == 0)
                {
                    _logger.LogDebug("Netsh command executed successfully: {Command}", command);
                    return true;
                }
                else
                {
                    _logger.LogError("Netsh command failed with exit code {ExitCode}. Error: {Error}", 
                        process.ExitCode, error);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing netsh command: {Command}", command);
                return false;
            }
        }

        private async Task<string?> ExecuteNetshCommandWithOutputAsync(string command)
        {
            try
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = "netsh",
                    Arguments = command,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using var process = Process.Start(processInfo);
                if (process == null) return null;

                await process.WaitForExitAsync();
                
                if (process.ExitCode == 0)
                {
                    return await process.StandardOutput.ReadToEndAsync();
                }
                else
                {
                    var error = await process.StandardError.ReadToEndAsync();
                    _logger.LogError("Netsh command failed with exit code {ExitCode}. Error: {Error}", 
                        process.ExitCode, error);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing netsh command with output: {Command}", command);
                return null;
            }
        }
    }
}
