using Avalonia.Media.Imaging;
using Barcoder;
using Barcoder.Pdf417;
using Barcoder.Qr;
using Barcoder.Renderer.Image;
using Dock.Model.Controls;
using p15.Core.Extensions;
using p15.Core.Messages;
using p15.Core.Models;
using p15.Core.Services;
using p15.Extensions;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.IO;

namespace p15.ViewModels
{
    public class BarcodesViewModel : Document
    {
        public string PackageName { get; internal set; }
        public ObservableCollection<BarcodeViewModel> Barcodes { get; internal set; }

        private Bitmap _bitmap;
        private string _barcode;
        private string _symbology;
        private int _uiScale;
        private int _fontSize;
        private int _largeFontSize;
        private IMessagingService _messagingService;
        private readonly TraceService _traceService;
        private readonly ClipboardService _clipboardService;

        public Bitmap Bitmap
        {
            get => _bitmap;
            set => this.RaiseAndSetIfChanged(ref _bitmap, value);
        }

        public string Barcode
        {
            get => _barcode;
            set
            {
                this.RaiseAndSetIfChanged(ref _barcode, value);
                RenderBarcode();
            }
        }

        public int FontSize
        {
            get => _fontSize;
            set => this.RaiseAndSetIfChanged(ref _fontSize, value);
        }

        public int LargeFontSize
        {
            get => _largeFontSize;
            set => this.RaiseAndSetIfChanged(ref _largeFontSize, value);
        }

        public int UiScale
        {
            get => _uiScale;
            set
            {
                this.RaiseAndSetIfChanged(ref _uiScale, value);
                FontSize = FontSizes.Text.Scale(_uiScale);
                LargeFontSize = FontSizes.LargeText.Scale(_uiScale);
            }
        }

        public BarcodesViewModel(
            IMessagingService messagingService,
            TraceService traceService,
            ClipboardService clipboardService,
            p15Model p15Model)
        {
            _messagingService = messagingService;

            messagingService.Subscribe<SendBarcodeMessage>(msg =>
            {
                _symbology = msg.Symbology;
                Barcode = msg.Barcode;
            });

            messagingService
                .SubscribeOnUIThread<UiScaleChangedMessage>(msg =>
                {
                    UiScale = msg.UiScale;
                });

            UiScale = p15Model.UiScale;

            _traceService = traceService;
            _clipboardService = clipboardService;
        }

        private void RenderBarcode()
        {
            IBarcode barcoder = _symbology.ToLower() switch
            {
                "qrcode" => QrEncoder.Encode(_barcode, ErrorCorrectionLevel.H, Encoding.Auto),
                "pdf417" => Pdf417Encoder.Encode(_barcode, 3),
                _ => null
            };

            if (barcoder == null)
            {
                _traceService.Warn($"Cannot render barcode. Symbology = {_symbology}, Barcode = {_barcode}");
                return;
            }

            var renderer = new ImageRenderer();

            using (var stream = new MemoryStream())
            {
                renderer.Render(barcoder, stream);
                stream.Position = 0;

                Bitmap = new Bitmap(stream);
            }
        }

        public void CopyToClipboard()
        {
            _clipboardService.CopyToCliboard(Barcode);
        }

        public void ScanBarcode()
        {
            _messagingService
                .SendMessage(new SendBarcodeMessage
                {
                    PackageName = PackageName,
                    Barcode = _barcode,
                    Symbology = _symbology
                });
        }
    }
}
