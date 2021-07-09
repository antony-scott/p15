using p15.Core.Messages;
using p15.Core.Models;
using System.Diagnostics;
using System.Linq;

namespace p15.Core.Services
{
    public class BarcodeService : IListen
    {
        private readonly p15Model _model;
        private readonly IMessagingService _messagingService;
        private readonly NetworkService _networkService;
        private readonly TextReplacementService _textReplacementService;

        public BarcodeService(
            p15Model model,
            IMessagingService messagingService,
            NetworkService networkService,
            TextReplacementService textReplacementService)
        {
            _model = model;
            _messagingService = messagingService;
            _networkService = networkService;
            _textReplacementService = textReplacementService;
        }

        public void Listen()
        {
            _messagingService
                .Subscribe<SendBarcodeMessage>(msg =>
                {
                    var intent = _model
                        .Intents
                        .FirstOrDefault(intent =>
                            intent.PackageName == msg.PackageName &&
                            intent.Name.ToLower() == "barcode");

                    if (intent != null)
                    {
                        var cmd = intent
                            .Command
                            .Replace("{value}", msg.Barcode)
                            .Replace("{symbology}", msg.Symbology);

                        var process = new Process
                        {
                            StartInfo = new ProcessStartInfo
                            {
                                FileName = "adb",
                                Arguments = cmd,
                                CreateNoWindow = true
                            }
                        };
                        process.Start();
                        process.WaitForExit();
                    }
                });
        }
    }
}
