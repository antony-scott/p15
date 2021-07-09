namespace p15.Core.Messages
{
    public class SendBarcodeMessage
    {
        public string PackageName { get; set; }
        public string Barcode { get; set; }
        public string Symbology { get; set; }
    }
}
