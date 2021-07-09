using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using p15.Core.Builders;
using p15.Core.Messages;
using p15.Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace p15.Core.Services
{
    public class ProcessService : IListen
    {
        private readonly IMessagingService _messagingService;
        private readonly TraceService _traceService;
        private readonly PowershellService _powershellService;
        private readonly AppSettings _appSettings;
        private readonly Dictionary<string, int> _managedProcesses = new Dictionary<string, int>(0);

        public ProcessBuilder Run(string filename)
        {
            return new ProcessBuilder(filename);
        }

        public ProcessService(
            IMessagingService messagingService,
            TraceService traceService,
            PowershellService powershellService,
            IOptions<AppSettings> settings)
        {
            _messagingService = messagingService;
            _traceService = traceService;
            _powershellService = powershellService;
            _appSettings = settings.Value;
        }

        public void Listen()
        {
            _messagingService
                .Subscribe<PerformStartupTasksMessage>(async msg =>
                {
                    if (_appSettings.StartupTasks != null)
                    {
                        foreach (var startupTask in _appSettings.StartupTasks)
                        {
                            await _powershellService.ExecuteAsync(ps =>
                            {
                                ps.AddScript(startupTask);
                            });
                        }
                    }
                });

            _messagingService
                .Subscribe<OpenTerminalMessage>(msg =>
                {
                    var terminal = string.IsNullOrWhiteSpace(_appSettings.Terminal)
                        ? "pwsh"
                        : _appSettings.Terminal;

                    Process.Start(new ProcessStartInfo(terminal)
                    {
                        UseShellExecute = true,
                        WorkingDirectory = msg.Path
                    });
                });

            _messagingService
                .Subscribe<StopProcessMessage>(msg =>
                {
                    try
                    {
                        var process = Process.GetProcessById(msg.ProcessId);
                        process.Kill();
                    }
                    catch
                    {
                    }
                    _messagingService.SendMessage(new ProcessStoppedMessage { ApplicationId = msg.ApplicationId });
                });

            _messagingService
                .Subscribe<ShellExecuteMessage>(msg =>
                {
                    var psi = new ProcessStartInfo(msg.Filename)
                    {
                        UseShellExecute = true,
                        WorkingDirectory = Path.GetDirectoryName(msg.Filename)
                    };
                    Process.Start(psi);
                });
        }

        internal async Task<WebApplicationInfo> StartWebApplication(string path)
        {
            var webApplicationInfo = await _powershellService.ExecuteAsync<WebApplicationInfo>(ps =>
            {
                ps.AddCommand("Start-WebApplication");
                ps.AddParameter("Path", path);
            });
            return webApplicationInfo;
        }

        internal async Task<EsbApplicationInfo> StartEsbApplication(string path, IEnumerable<string> arguments)
        {
            var esbApplicationInfo = await _powershellService.ExecuteAsync<EsbApplicationInfo>(ps =>
            {
                ps.AddCommand("Start-EsbService");
                ps.AddParameter("Path", path);
                ps.AddParameter("Arguments", arguments);
            });
            return esbApplicationInfo;
        }

        public void RegisterManagedProcess(string name, int processId)
        {
            if (_managedProcesses.ContainsKey(name))
            {
                _managedProcesses[name] = processId;
            }
            else
            {
                _managedProcesses.Add(name, processId);
            }
        }

        public void KillManagedProcesses()
        {
            foreach (var key in _managedProcesses.Keys)
            {
                var processId = _managedProcesses[key];
                _traceService.Info($"Killing {key} process (process id = {processId})");
                try
                {
                    var process = Process.GetProcessById(processId);
                    process.Kill();
                }
                catch (Exception ex)
                {
                    _traceService.Error($"There was a problem killing process (id = {processId}). {ex.Message}");
                }
            }
        }

        public async Task<bool> IsProcessRunning(int processId)
        {
            var processResult = await _powershellService.ExecuteAsync<ProcessIdResult>(ps =>
            {
                ps.AddCommand("Get-ProcessIsRunning");
                ps.AddParameter("ProcessId", processId);
            });
            return processResult?.ProcessId == processId;
        }
    }
}
