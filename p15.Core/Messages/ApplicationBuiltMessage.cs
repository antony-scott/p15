using System;

namespace p15.Core.Messages
{
    public class ApplicationBuiltMessage
    {
        public Guid ApplicationId { get; set; }
        public string PackageName { get; set; }
        public string ApplicationName { get; set; }
        public bool Bootstrapping { get; set; }
    }
}
