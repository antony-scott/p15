namespace p15.Core.Models
{
    public class IdentityServer
    {
        public string[] Scopes { get; set; }
        public bool RequiresExternalAccess { get; set; }
        public int Port { get; set; }
    }
}
