using p15.Core.Extensions;
using p15.Core.Messages;
using p15.Core.Services;
using p15.Extensions;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using System;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using p15.Core.Parsers;
using System.Collections.Generic;

namespace p15.ViewModels
{
    public class AppViewModel : ReactiveObject
    {
        public AppViewModel(
            IMessagingService messagingService,
            ProcessService processService,
            TraceService traceService)
        {
            _messagingService = messagingService;
            _processService = processService;
            _traceService = traceService;

            ApplicationId = Guid.NewGuid();

            messagingService
                .SubscribeOnUIThread<ApplicationPreparedMessage>(msg =>
                {
                    if (msg.ApplicationId != ApplicationId) return;

                    ProjectFolderExists = msg.ProjectFolderExists;
                    HasLogging = msg.HasLogging;
                    LogsFolder = msg.LogsFolder;
                    LogFilenameFilter = msg.LogFilenameFilter;
                    Path = msg.Path;
                });

            messagingService
                .SubscribeOnUIThread<WebServiceStartedMessage>(msg =>
                {
                    if (msg.ApplicationId != ApplicationId) return;

                    _traceService.Info($"Web Service Started => {Name} on port {msg.Port}");

                    ProcessId = msg.ProcessId;
                    PortNumber = msg.Port;
                    MonitorLogs();
                });

            messagingService
                .SubscribeOnUIThread<EsbServiceStartedMessage>(msg =>
                {
                    if (msg.ApplicationId != ApplicationId) return;

                    _traceService.Info($"{Name} Started (Process Id = {msg.ProcessId}) => {msg.Filename} {msg.Arguments}");

                    ProcessId = msg.ProcessId;
                    MonitorLogs();
                });

            messagingService
                .SubscribeOnUIThread<ProcessStoppedMessage>(msg =>
                {
                    if (msg.ApplicationId != ApplicationId) return;

                    ProcessId = null;
                });

            messagingService
                .SubscribeOnUIThread<ApplicationIsRunningMessage>(msg =>
                {
                    if (msg.ApplicationId != ApplicationId) return;

                    ProcessId = msg.ProcessId;

                    messagingService
                        .SendMessage(new MonitorLogsMessage
                        {
                            Name = Name,
                            LogsFolder = LogsFolder,
                            LogFilenameFilter = LogFilenameFilter
                        });
                });

            messagingService
                .SubscribeOnUIThread<ApplicationSyncedMessage>(msg =>
                {
                    if (msg.ApplicationId != ApplicationId) return;
                    Refresh();
                    if (msg.Bootstrapping)
                    {
                        messagingService.SendMessage(new ProcessMessageQueuesMessage
                        {
                            ApplicationId = msg.ApplicationId,
                            PackageName = msg.PackageName,
                            ApplicationName = msg.ApplicationName,
                            Path = Path,
                            Bootstrapping = true
                        });
                    }
                });

            messagingService
                .SubscribeOnUIThread<MessageQueuesProcessedMessage>(msg =>
                {
                    if (msg.ApplicationId != ApplicationId) return;
                    if (msg.Bootstrapping)
                    {
                        messagingService.SendMessage(new BuildApplicationMessage
                        {
                            ApplicationId = msg.ApplicationId,
                            PackageName = msg.PackageName,
                            ApplicationName = msg.ApplicationName,
                            Path = Path,
                            Bootstrapping = true
                        });
                    }
                });

            messagingService
                .SubscribeOnUIThread<ApplicationBuiltMessage>(msg =>
                {
                    if (msg.ApplicationId != ApplicationId) return;
                    Refresh();
                });
        }

        public Guid ApplicationId { get; }
        public string PackageName { get; internal set; }
        public string Name { get; set; }
        public string Project { get; set; }
        public string ApplicationType { get; set; }
        public string Source { get; set; }
        public bool RequiresExternalAccess { get; set; }
        public IEnumerable<string> Tags { get; set; }
        public string AnonymousAuthentication { get; set; }
        public string BasicAuthentication { get; set; }
        public string Logs { get; set; }

        public bool HasCustomLogging => !string.IsNullOrWhiteSpace(Logs);

        public bool IsRunning => ProcessId.HasValue;

        private int? _processId;
        public int? ProcessId
        {
            get => _processId;
            set
            {
                this.RaiseAndSetIfChanged(ref _processId, value);
                this.RaisePropertyChanged(nameof(IsRunning));
                this.RaisePropertyChanged(nameof(ProcessCanBeStarted));
                this.RaisePropertyChanged(nameof(ProcessCanBeStopped));
            }
        }

        public bool IsWeb => PortNumber.HasValue;

        private int? _portNumber;
        public int? PortNumber
        {
            get => _portNumber;
            set
            {
                this.RaiseAndSetIfChanged(ref _portNumber, value);
                this.RaisePropertyChanged(nameof(IsWeb));
            }
        }

        private string _path;

        public string Path
        {
            get => _path;
            set
            {
                _path = value;
                Refresh();
            }
        }

        public IEnumerable<string> Arguments { get; set; }

        public string LogsFolder { get; set; }
        public string LogFilenameFilter { get; set; }

        private bool _pathExists;
        public bool PathExists
        {
            get => _pathExists;
            set => this.RaiseAndSetIfChanged(ref _pathExists, value);
        }

        private bool _projectFolderExists;
        public bool ProjectFolderExists
        {
            get => _projectFolderExists;
            set => this.RaiseAndSetIfChanged(ref _projectFolderExists, value);
        }

        public string BinariesFolder { get; set; }

        private bool _binariesExist;

        public bool BinariesExist
        {
            get => _binariesExist;
            set
            {
                this.RaiseAndSetIfChanged(ref _binariesExist, value);
                this.RaisePropertyChanged(nameof(ProcessCanBeStarted));
                this.RaisePropertyChanged(nameof(ProcessCanBeStopped));
            }
        }

        private bool _hasLogging;
        private readonly IMessagingService _messagingService;
        private readonly ProcessService _processService;
        private readonly TraceService _traceService;

        public bool HasLogging
        {
            get => _hasLogging;
            internal set => this.RaiseAndSetIfChanged(ref _hasLogging, value);
        }

        public string SolutionFilename { get; private set; }

        public string SourceType => Source.EndsWith(".git") ? "git" : "hg";

        public bool ProcessIsStartable => new[] { "web", "esb" }.Contains(ApplicationType.ToLower());
        public bool ProcessCanBeStarted => ProcessIsStartable && BinariesExist && !ProcessId.HasValue;
        public bool ProcessCanBeStopped => ProcessIsStartable && ProcessId.HasValue;

        public void Refresh()
        {
            PathExists = Directory.Exists(Path);
            if (PathExists)
            {
                RefreshProjectFolder();
                RefreshBinaries();
                RefreshSolutionFile();
            }

            _messagingService.SendMessage(new CheckIfApplicationIsRunningMessage
            {
                ApplicationId = ApplicationId,
                Path = Path,
                ApplicationType = ApplicationType,
                Project = Project
            });
        }

        private void RefreshProjectFolder()
        {
            var projectFolder = System.IO.Path.Combine(Path, Project);
            ProjectFolderExists = Directory.Exists(projectFolder);
        }

        private void RefreshBinaries()
        {
            if (!PathExists) return;

            BinariesFolder = null;

            switch (ApplicationType.ToLower())
            {
                case "esb":
                    var hostExeFiles = Directory
                        .GetFiles(Path, "NServiceBus.Host.exe", SearchOption.AllDirectories);
                    var hostFilename = hostExeFiles.FirstOrDefault(x => x.Contains(@"\Debug\") && !x.Contains("Tests"));

                    if (hostFilename == null)
                    {
                        hostFilename = Directory
                            .GetFiles(Path, $"{Project}.exe", SearchOption.AllDirectories)
                            .FirstOrDefault(x => x.Contains(@"\Debug\") &&
                                                 !x.Contains("Tests"));
                    }
                    BinariesFolder = hostFilename.GetContainingFolder();
                    break;

                case "web":
                    var dllFilename = Directory
                        .GetFiles(Path, $"{Project}.dll", SearchOption.AllDirectories)
                        .FirstOrDefault(x => x.Contains("\\bin\\"));
                    BinariesFolder = dllFilename.GetContainingFolder();
                    break;

                case "tool":
                    var toolFilename = Directory
                        .GetFiles(Path, $"{Project}.exe", SearchOption.AllDirectories)
                        .FirstOrDefault(x => x.Contains(@"\Debug\") && !x.Contains("Tests"));
                    BinariesFolder = toolFilename.GetContainingFolder();
                    break;
            }

            BinariesExist = BinariesFolder != null;
        }

        private void RefreshSolutionFile()
        {
            var solutionFilenames = Directory
                .GetFiles(Path, "*.sln", SearchOption.AllDirectories);

            SolutionFilename = solutionFilenames.Length == 1
                ? solutionFilenames.First()
                : solutionFilenames.FirstOrDefault(x => x.Contains($"{Project}.sln"));
        }

        public void Start()
        {
            object message = ApplicationType.ToLower() switch
            {
                "web" => new StartWebServiceMessage { ApplicationId = ApplicationId, Name = Name, Path = Path },
                "esb" => new StartEsbServiceMessage { ApplicationId = ApplicationId, Name = Name, Path = Path, Arguments = Arguments },
                _ => null
            };

            if (message != null)
            {
                _messagingService.SendMessage(message);
            }
        }

        public void Stop() => _messagingService.SendMessage(new StopProcessMessage
        {
            ApplicationId = ApplicationId,
            ProcessId = ProcessId.Value
        });

        public void Build() => _messagingService.SendMessage(new BuildApplicationMessage
        {
            ApplicationId = ApplicationId,
            Path = Path
        });

        public void PurgeBinObjFolders()
        {
            var folders = Directory
                .GetDirectories(Path, "*", SearchOption.AllDirectories)
                .Where(x => x.EndsWith(@"\bin") || x.EndsWith(@"\obj"))
                .ToArray();
            foreach (var folder in folders)
            {
                _traceService.Info($"Purging folder {folder}");
                Directory.Delete(folder, true);
            }
        }

        public void Terminal() => _messagingService.SendMessage(new OpenTerminalMessage
        {
            Path = Path
        });

        public void TerminalInBinariesFolder() => _messagingService.SendMessage(new OpenTerminalMessage
        {
            Path = BinariesFolder
        });

        public void OpenFolder() => _messagingService.SendMessage(new OpenFolderMessage
        {
            Folder = Path
        });

        public void OpenLogsFolder() => _messagingService.SendMessage(new OpenFolderMessage
        {
            Folder = LogsFolder
        });

        public void OpenBinariesFolder() => _messagingService.SendMessage(new OpenFolderMessage
        {
            Folder = BinariesFolder
        });

        public void BrowseSource() => _messagingService.SendMessage(new ShellExecuteMessage
        {
            Filename = Source
        });

        public void Go() => _messagingService.SendMessage(new BootstrapApplicationMessage
        {
            ApplicationId = ApplicationId,
            PackageName = PackageName,
            ApplicationName = Name,
            Path = Path,
            Source = Source
        });

        public void PullFolder() => _messagingService.SendMessage(new PullApplicationFolderMessage
        {
            ApplicationId = ApplicationId,
            PackageName = PackageName,
            ApplicationName = Name,
            Path = Path,
            Url = Source
        });

        public void PushFolder() => _messagingService.SendMessage(new PushApplicationFolderMessage
        {
            ApplicationId = ApplicationId,
            PackageName = PackageName,
            ApplicationName = Name,
            Path = Path,
            Url = Source
        });

        public void OpenIDE() => _messagingService.SendMessage(new ShellExecuteMessage
        {
            Filename = SolutionFilename
        });

        public void MessageQueues() => _messagingService.SendMessage(new ProcessMessageQueuesMessage
        {
            PackageName = PackageName,
            ApplicationName = Name,
            Path = Path
        });

        public void ViewReadme()
        {
            var filename = Directory
                .GetFiles(Path, "readme.md", SearchOption.TopDirectoryOnly)
                .FirstOrDefault();

            if (filename != null)
            {
                _messagingService.SendMessage(new ViewMarkdownMessage
                {
                    Name = Name,
                    Filename = filename
                });
            }
        }

        public void MonitorLogs() => _messagingService.SendMessage(new MonitorLogsMessage
        {
            Name = Name,
            LogsFolder = LogsFolder,
            LogFilenameFilter = LogFilenameFilter
        });

        public void Browse() => _messagingService.SendMessage(new OpenWebPageMessage
        {
            Url = Source
        });

        public void StartCustomLogging()
        {
            var id = Guid.NewGuid();

            _messagingService.SendMessage(new MonitorCustomLogsMessage
            {
                Id = id,
                Name = Project
            });

            var parts = Logs.Split(' ');
            var cmd = parts.First();
            var args = string.Join(" ", parts.Skip(1).ToArray());

            var process = _processService
                .Run(cmd)
                .WithArguments(args)
                .WithoutCreatingAWindow()
                .CaptureStdOut((data) =>
                {
                    var parser = App.Services.GetService<AdbLogParser>();
                    var logEntry = parser.Parse(data);
                    if (logEntry != null)
                    {
                        _messagingService.SendMessage(new LogEntriesMessage
                        {
                            Identifier = id,
                            Name = Project,
                            LogEntries = new[] { logEntry }
                        });
                    }
                })
                .Start();

            _processService.RegisterManagedProcess($"{Project} custom logging process", process.Id);
        }
    }
}
