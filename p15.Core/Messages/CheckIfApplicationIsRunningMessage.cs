using System;

namespace p15.Core.Messages
{
    public class CheckIfApplicationIsRunningMessage : ApplicationMessage
    {
        public Guid ApplicationId { get; set; }
        public string Path { get; set; }
        public string ApplicationType { get; set; }
        public string Project { get; set; }
    }
}
