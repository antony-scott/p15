using System.Collections.Generic;

namespace p15.Core.Models
{
    public class Barcode
    {
        public string Name { get; set; }
        public string Symbology { get; set; }
        public Dictionary<string, string> Values { get; set; }
        public string PackageName { get; set; }
    }
}
