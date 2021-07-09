using p15.Core.Messages;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace p15.Core.Services
{
    public class NetworkService : IListen
    {
        private readonly IMessagingService _messagingService;

        public IPAddress LocalIpAddress { get; }
        public IPAddress AndroidHostLoopbackIpAddress { get; }

        public NetworkService(IMessagingService messagingService)
        {
            _messagingService = messagingService;

            LocalIpAddress = NetworkInterface
                .GetAllNetworkInterfaces()
                .Select(x => x.GetIPProperties())
                .SelectMany(x => x.UnicastAddresses)
                .FirstOrDefault(x =>
                    x.Address.AddressFamily == AddressFamily.InterNetwork &&
                    !IPAddress.IsLoopback(x.Address) &&
                    x.PrefixOrigin == PrefixOrigin.Dhcp)
                ?.Address;

            // TODO: pull in config override for this from config.json
            AndroidHostLoopbackIpAddress = IPAddress.Parse("10.0.0.2");
        }

        public void Listen()
        {
            _messagingService
                .Subscribe<OpenWebPageMessage>(msg =>
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = msg.Url,
                        UseShellExecute = true
                    });
                });
        }
    }
}
