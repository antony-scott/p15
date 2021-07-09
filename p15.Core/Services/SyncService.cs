using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using p15.Core.Messages;

namespace p15.Core.Services
{
    public class SyncService : IListen
    {
        private readonly PowershellService _powershellService;
        private readonly IMessagingService _messagingService;
        private readonly TraceService _traceService;
        private readonly IApplicationConfiguration _applicationConfiguration;
        private readonly AppSettings _appSettings;

        public SyncService(
            PowershellService powershellService,
            IMessagingService messagingService,
            TraceService traceService,
            IOptions<AppSettings> settings,
            IApplicationConfiguration applicationConfiguration)
        {
            _powershellService = powershellService;
            _messagingService = messagingService;
            _traceService = traceService;
            _applicationConfiguration = applicationConfiguration;
            _appSettings = settings.Value;
        }

        public async Task PullFolder(string path, string url)
        {
            await _powershellService.ExecuteAsync(ps =>
            {
                ps.AddCommand("Pull-Folder");
                ps.AddParameter("Path", path);
                ps.AddParameter("Url", url);
            });
        }

        public async Task PushFolder(string path, string url)
        {
            await _powershellService.ExecuteAsync(ps =>
            {
                ps.AddCommand("Push-Folder");
                ps.AddParameter("Path", path);
                ps.AddParameter("Url", url);
            });
        }

        public void Listen()
        {
            _messagingService.Subscribe<PerformStartupTasksMessage>(async msg => await SyncManagedRepositories());
        }


        private async Task SyncManagedRepositories()
        {
            foreach (var managedRepository in _appSettings.ManagedRepositories)
            {
                _traceService.Info($"Syncing {managedRepository.Name} repository");
                var folder = Path.Combine(_applicationConfiguration.RootFolder, managedRepository.Folder);
                await PullFolder(folder, managedRepository.Url);
            }
        }
    }
}
