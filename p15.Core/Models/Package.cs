using System.Collections.Generic;

namespace p15.Core.Models
{
    public class Package
    {
        public string Name { get; set; }
        public App[] Applications { get; set; }
        public Databases Databases { get; set; }
        public Dictionary<string, string> Bookmarks { get; set; }
        public IdentityServer IdentityServer { get; set; }
        public AndroidEmulator AndroidEmulator { get; set; }
        public Dictionary<string, string> Logs { get; set; }
        public Intent[] Intents { get; set; }
        public Barcode[] Barcodes { get; set; }
        public Dictionary<string, string> SqlScripts { get; set; }
    }
}
