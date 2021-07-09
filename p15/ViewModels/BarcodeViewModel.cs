using p15.Core.Messages;
using p15.Core.Services;
using ReactiveUI;
using System.Collections.Generic;

namespace p15.ViewModels
{
    public class BarcodeViewModel : ReactiveObject
    {
        private readonly IMessagingService _messagingService;
        private readonly NetworkService _networkService;
        private readonly TextReplacementService _textReplacementService;

        public BarcodeViewModel(
            IMessagingService messagingService,
            NetworkService networkService,
            TextReplacementService textReplacementService)
        {
            _messagingService = messagingService;
            _networkService = networkService;
            _textReplacementService = textReplacementService;
        }

        public string PackageName { get; set; }
        public string Name { get; set; }
        public string Symbology { get; set; }
        public Dictionary<string, string> Values { get; set; }

        public void SendCommand(string key)
        {
            var barcode = Values[key]
                .Replace("{ipaddress}", _networkService.LocalIpAddress.ToString())
                .Replace("{android-host-loopback}", _networkService.AndroidHostLoopbackIpAddress.ToString());

            barcode = _textReplacementService.ReplaceDates(barcode);

            _messagingService
                .SendMessage(new SendBarcodeMessage
                {
                    PackageName = PackageName,
                    Barcode = barcode,
                    Symbology = Symbology
                });
        }
    }
}
