using Microsoft.Extensions.Options;
using p15.Core.Messages;
using RestSharp;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace p15.Core.Services
{
    public class RavenService : IListen
    {
        private readonly TraceService _traceService;
        private readonly IMessagingService _messagingService;
        private readonly AppSettings _settings;

        public RavenService(
            TraceService traceService,
            IOptions<AppSettings> settings,
            IMessagingService messagingService)
        {
            _traceService = traceService;
            _messagingService = messagingService;
            _settings = settings.Value;
        }

        public void Listen()
        {
            _messagingService.Subscribe<PerformStartupTasksMessage>(async msg => await EnsureRavenIsRunning());
        }

        public async Task EnsureRavenIsRunning()
        {
            await StartRavenInstances();
        }

        private async Task StartRavenInstances()
        {
            var tasks = _settings.RavenServers.Select(x => StartRaven(x.Location, x.Version, x.Port));
            await Task.WhenAll(tasks);
        }

        private Process CreateProcess(string filename, string arguments = "", bool useShellExecute = true, bool createNoWindow = false)
        {
            if (string.IsNullOrWhiteSpace(filename) || !File.Exists(filename)) return null;

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    WorkingDirectory = Path.GetDirectoryName(filename) ?? "",
                    FileName = filename,
                    Arguments = arguments,
                    UseShellExecute = useShellExecute,
                    CreateNoWindow = createNoWindow
                }
            };

            return process;
        }

        private Task StartRaven(string serverExeLocation, int version, int port)
        {
            return Task.Run(async () =>
            {
                var processStarted = false;
                var client = new RestClient($"http://localhost:{port}");
                var request = new RestRequest("/build/version");
                var started = false;
                var retryCount = 0;
                while (!started && retryCount <= 3)
                {
                    var response = client.Get(request);
                    _traceService.Info($"StartRaven v{version} on port {port} => {response.Content}");
                    started = response.StatusCode == HttpStatusCode.OK;
                    if (!started)
                    {
                        ++retryCount;
                        if (!processStarted)
                        {
                            var process = CreateProcess(serverExeLocation, $"--set=Raven/Port=={port}");
                            process.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
                            process = Process.Start(process.StartInfo);
                            processStarted = process != null;
                            await Task.Delay(3000);
                        }
                        await Task.Delay(100);
                    }
                }
            });
        }

        public void StopAll()
        {
            var ravens = Process.GetProcessesByName("Raven.Server.exe");
            foreach (var raven in ravens)
            {
                raven.Kill();
            }
        }
    }
}
