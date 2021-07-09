using p15.Core.Messages;
using p15.Core.Models;
using System.IO;
using System.Linq;
using System.Xml;
using System;
using System.Threading.Tasks;
using System.Security.Policy;

namespace p15.Core.Services
{
    public sealed class AppService : IListen
    {
        private readonly IMessagingService _messagingService;
        private readonly IApplicationConfiguration _applicationConfiguration;
        private readonly TraceService _traceService;
        private readonly MsmqService _msmqService;
        private readonly SyncService _syncService;
        private readonly ProcessService _processService;
        private readonly BuildService _buildService;
        private readonly PowershellService _powershellService;

        public p15Model Model { get; }

        public AppService(
            p15Model model,
            IMessagingService messagingService,
            IApplicationConfiguration applicationConfiguration,
            TraceService traceService,
            MsmqService msmqService,
            SyncService syncService,
            ProcessService processService,
            BuildService buildService,
            PowershellService powershellService)
        {
            Model = model;

            _messagingService = messagingService;
            _applicationConfiguration = applicationConfiguration;
            _traceService = traceService;
            _msmqService = msmqService;
            _syncService = syncService;
            _processService = processService;
            _buildService = buildService;
            _powershellService = powershellService;
        }

        public void Listen()
        {
            _messagingService
                .Subscribe<StartWebServiceMessage>(async msg =>
                {
                    var webApplicationInfo = await _processService.StartWebApplication(msg.Path);
                    if (webApplicationInfo != null)
                    {
                        _messagingService.SendMessage(new WebServiceStartedMessage
                        {
                            ApplicationId = msg.ApplicationId,
                            ProcessId = webApplicationInfo.ProcessId,
                            Port = webApplicationInfo.Port
                        });
                    }
                    else
                    {
                        _traceService.Error($"Failed to start {msg.Name}");
                    }
                });

            _messagingService
                .Subscribe<StartEsbServiceMessage>(async msg =>
                {
                    var esbApplicationInfo = await _processService.StartEsbApplication(msg.Path, msg.Arguments);
                    if (esbApplicationInfo != null)
                    {
                        _messagingService.SendMessage(new EsbServiceStartedMessage
                        {
                            ApplicationId = msg.ApplicationId,
                            ProcessId = esbApplicationInfo.ProcessId,
                        });
                    }
                    else
                    {
                        _traceService.Error($"Failed to start {msg.Name}");
                    }
                });

            _messagingService
                .Subscribe<ProcessMessageQueuesMessage>(async msg =>
                {
                    if (msg.Bootstrapping)
                    {
                        _traceService.Info($"Configuring MSMQs for {msg.PackageName} > {msg.ApplicationName}");
                    }

                    await _msmqService.ConfigureQueues(msg.Path);

                    if (msg.Bootstrapping)
                    {
                        _messagingService.SendMessage(new MessageQueuesProcessedMessage
                        {
                            ApplicationId = msg.ApplicationId,
                            PackageName = msg.PackageName,
                            ApplicationName = msg.ApplicationName,
                            Bootstrapping = true
                        });
                    }
                });

            _messagingService
                .Subscribe<BuildApplicationMessage>(async msg =>
                {
                    if (msg.Bootstrapping)
                    {
                        _traceService.Info($"Building {msg.PackageName} > {msg.ApplicationName}");
                    }
                    
                    await Build(msg.ApplicationId, msg.Path, msg.Bootstrapping);

                    _messagingService.SendMessage(new ApplicationBuiltMessage
                    {
                        ApplicationId = msg.ApplicationId,
                        PackageName = msg.PackageName,
                        ApplicationName = msg.ApplicationName,
                        Bootstrapping = msg.Bootstrapping
                    });
                });

            _messagingService
                .Subscribe<PullApplicationFolderMessage>(async msg =>
                {
                    await PullFolder(msg.ApplicationId, msg.PackageName, msg.ApplicationName, msg.Path, msg.Url);
                });

            _messagingService
                .Subscribe<PushApplicationFolderMessage>(async msg =>
                {
                    await _syncService.PushFolder(msg.Path, msg.Url);
                });

            _messagingService
                .Subscribe<BootstrapApplicationMessage>(async msg =>
                {
                    _traceService.Info($"Syncing {msg.PackageName} > {msg.ApplicationName}");
                    await PullFolder(msg.ApplicationId, msg.PackageName, msg.ApplicationName, msg.Path, msg.Source, bootstrapping: true);
                });

            _messagingService
                .Subscribe<PrepareApplicationMessage>(msg =>
                {
                    ProcessApp(msg.ApplicationId, msg.ApplicationName, msg.ProjectName);
                });

            _messagingService
                .Subscribe<CheckIfApplicationIsRunningMessage>(msg =>
                {
                    var cmd = msg.ApplicationType.ToLower() switch
                    {
                        "web" => "Get-WebProcessId",
                        "esb" => "Get-EsbProcessId",
                        _ => null
                    };

                    if (cmd != null)
                    {
                        var result = _powershellService.Execute<ProcessIdResult>(ps =>
                        {
                            ps.AddCommand(cmd);
                            ps.AddParameter("Project", msg.Project);
                        });
                        if (result != null)
                        {
                            _messagingService
                                .SendMessage(new ApplicationIsRunningMessage
                                {
                                    ApplicationId = msg.ApplicationId,
                                    ProcessId = result.ProcessId
                                });
                        }
                    }
                });
        }

        public void ProcessApp(Guid applicationId, string appName, string projectName)
        {
            var path = Path.Combine(_applicationConfiguration.RootFolder, projectName);

            var projectFolderExists = false;
            var hasLogging = false;
            string logsFolder = null;
            string logFilenameFilter = null;

            if (Directory.Exists(path))
            {
                var projectFolder = Path.Combine(path, projectName);
                projectFolderExists = Directory.Exists(projectFolder);
                if (projectFolderExists)
                {
                    var configFilename = Directory
                        .GetFiles(projectFolder, "*.config")
                        .FirstOrDefault(x =>
                        {
                            var filename = Path.GetFileName(x).ToLower();
                            return filename == "app.config" || filename == "web.config";
                        });

                    if (configFilename != null)
                    {
                        var xmlDoc = new XmlDocument();
                        xmlDoc.Load(configFilename);

                        var xpathQuery = "//add[translate(@key,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz') = 'logfile']/@value";
                        var result = xmlDoc.SelectSingleNode(xpathQuery);
                        if (result != null)
                        {
                            var logfilename = result.Value;

                            hasLogging = true;
                            logsFolder = Path.GetDirectoryName(logfilename);
                            logFilenameFilter = $"{Path.GetFileNameWithoutExtension(logfilename)}*{Path.GetExtension(logfilename)}";

                            if (!Directory.Exists(logsFolder))
                            {
                                _traceService.Info($"Creating logs folder for {appName} ({logsFolder})");
                                Directory.CreateDirectory(logsFolder);
                            }
                        }
                    }
                }

                // TODO: process commands section of application
                // TODO: add commands to context menu of application
            }

            _messagingService
                .SendMessage(new ApplicationPreparedMessage
                {
                    ApplicationId = applicationId,
                    Path = path,
                    ProjectFolderExists = projectFolderExists,
                    HasLogging = hasLogging,
                    LogsFolder = logsFolder,
                    LogFilenameFilter = logFilenameFilter
                });
        }

        private async Task Build(Guid applicationId, string path, bool bootstrapping = false)
        {
            await _buildService.Build(path);
        }

        private async Task PullFolder(Guid applicationId, string packageName, string applicationName, string path, string url, bool bootstrapping = false)
        {
            await _syncService.PullFolder(path, url);
            _messagingService.SendMessage(new ApplicationSyncedMessage
            {
                ApplicationId = applicationId,
                PackageName = packageName,
                ApplicationName = applicationName,
                Bootstrapping = bootstrapping
            });
        }
    }
}
