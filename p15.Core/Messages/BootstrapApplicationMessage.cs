using System;

namespace p15.Core.Messages
{
    public class BootstrapApplicationMessage
    {
        public Guid ApplicationId { get; set; }
        public string PackageName { get; set; }
        public string ApplicationName { get; set; }
        public string Path { get; set; }
        public string Source { get; set; }
    }
}
