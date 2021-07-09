namespace p15.Core.Messages
{
    public class BootstrapMessage
    {
        public string PackageName { get; set; }
        public string ApplicationName { get; set; }
        public bool Bootstrapping { get; set; } = false;
    }
}
