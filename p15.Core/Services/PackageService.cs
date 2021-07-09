using Microsoft.Extensions.Options;
using p15.Core.Messages;
using p15.Core.Models;
using SharpYaml;
using SharpYaml.Serialization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using p15.Extensions;

namespace p15.Core.Services
{
    public class PackageService : IListen
    {
        private readonly IApplicationConfiguration _applicationConfiguration;
        private readonly p15Model _p15Model;
        private readonly IMessagingService _messagingService;
        private readonly MsmqService _msmqService;
        private readonly SyncService _syncService;
        private readonly TraceService _traceService;
        private readonly FirewallService _firewallService;
        private readonly AppSettings _appSettings;

        public PackageService(
            IApplicationConfiguration applicationConfiguration,
            p15Model p15Model,
            IMessagingService messagingService,
            MsmqService msmqService,
            IOptions<AppSettings> settings,
            SyncService syncService,
            TraceService traceService,
            FirewallService firewallService)
        {
            _applicationConfiguration = applicationConfiguration;
            _p15Model = p15Model;
            _messagingService = messagingService;
            _msmqService = msmqService;
            _syncService = syncService;
            _traceService = traceService;
            _firewallService = firewallService;
            _appSettings = settings.Value;
        }

        public void Listen()
        {
            _messagingService
                .SubscribeOnUIThread<PerformStartupTasksMessage>(async msg => await LoadPackages());

            _messagingService
                .Subscribe<LoadPackageMessage>(async msg =>
                {
                    await ProcessMessageQueues(msg.PackageName);
                    ProcessCustomLogs(msg.PackageName);
                    ProcessExtraLogFolders(msg.PackageName);
                    await ProcessFirewallRules(msg.PackageName);
                });
        }

        private async Task LoadPackages()
        {
            var serializer = new Serializer(
                new SerializerSettings
                {
                    NamingConvention = new FlatNamingConvention()
                });

            var folder = Path.Combine(_applicationConfiguration.RootFolder, FileSystemService.PackagesFolderName);
            await _syncService.PullFolder(folder, _appSettings.PackagesRepository);

            var packageFiles = Directory.GetFiles(folder);

            foreach (var packageFile in packageFiles)
            {
                Package package = null;

                try
                {
                    var text = File.ReadAllText(packageFile);
                    package = serializer.Deserialize<Package>(text);
                }
                catch (YamlException ex)
                {
                    _traceService.Error($"Cannot load {packageFile} - {ex.Message}");
                }

                if (package != null)
                {
                    _p15Model.PackageNames.Add(package.Name);

                    foreach (var application in package.Applications)
                    {
                        application.PackageName = package.Name;
                        _p15Model.Applications.Add(application);
                    }

                    if (package.Barcodes?.Any() ?? false)
                    {
                        foreach (var barcode in package.Barcodes)
                        {
                            barcode.PackageName = package.Name;
                            _p15Model.Barcodes.Add(barcode);
                        }
                    }

                    if (package.Intents?.Any() ?? false)
                    {
                        foreach (var intent in package.Intents)
                        {
                            intent.PackageName = package.Name;
                            _p15Model.Intents.Add(intent);
                        }
                    }

                    if (package.Bookmarks?.Any() ?? false)
                    {
                        foreach (var bookmark in package.Bookmarks)
                        {
                            _p15Model.Bookmarks.Add(new Bookmark
                            {
                                PackageName = package.Name,
                                Name = bookmark.Key,
                                Url = bookmark.Value
                            });
                        }
                    }

                    if (package.Logs?.Any() ?? false)
                    {
                        foreach (var log in package.Logs)
                        {
                            _p15Model.Logs.Add(new Log
                            {
                                PackageName = package.Name,
                                Name = log.Key,
                                Filename = log.Value
                            });
                        }
                    }

                    if (package.IdentityServer != null)
                    {
                        if (package.IdentityServer.RequiresExternalAccess)
                        {
                            _p15Model.FirewallRules.Add(new FirewallRule
                            {
                                PackageName = package.Name,
                                Name = $"Identity Server",
                                Port = package.IdentityServer.Port
                            });
                        }
                    }
                }
            }
        }


        private async Task ProcessMessageQueues(string packageName)
        {
            var tasks = _p15Model
                .Applications
                .Where(app => app.PackageName == packageName)
                .Select(app => Path.Combine(_applicationConfiguration.RootFolder, app.Project))
                .Select(path => _msmqService.ConfigureQueues(path))
                .ToArray();

            await Task.WhenAll(tasks);
        }

        private void ProcessCustomLogs(string packageName)
        {
            var appsWithCustomLogs = _p15Model
                .Applications
                .Where(app => app.PackageName == packageName &&
                              !string.IsNullOrWhiteSpace(app.Logs))
                .ToArray();

            foreach (var app in appsWithCustomLogs)
            {
            }
        }

        private void ProcessExtraLogFolders(string packageName)
        {
            var jim = _p15Model
                .Logs
                .Where(x => x.PackageName == packageName)
                .ToArray();
        }

        private async Task ProcessFirewallRules(string packageName)
        {
            var firewallRules = _p15Model
                .FirewallRules
                .Where(x => x.PackageName == packageName)
                .ToArray();

            foreach (var rule in firewallRules)
            {
                await _firewallService.AddRule(rule);
            }

        }
    }
}
