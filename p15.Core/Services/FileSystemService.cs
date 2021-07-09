using p15.Core.Messages;
using System.Diagnostics;

namespace p15.Core.Services
{
    public class FileSystemService : IListen
    {
        private readonly IMessagingService _messagingService;

        public static string PackagesFolderName = ".p15-packages";

        public FileSystemService(IMessagingService messagingService)
        {
            _messagingService = messagingService;
        }

        public void Listen()
        {
            _messagingService
                .Subscribe<OpenFolderMessage>(msg =>
                {
                    Process.Start(new ProcessStartInfo(msg.Folder) { UseShellExecute = true });
                });
        }
    }
}
