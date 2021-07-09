namespace p15.Core.Models
{
    public class AndroidEmulator
    {
        public string Name { get; set; }
        public string LogTag { get; set; }
        public PortRedirection[] PortRedirections { get; set; }
    }
}
