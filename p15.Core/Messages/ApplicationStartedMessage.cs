using System;

namespace p15.Core.Messages
{
    public class ApplicationStartedMessage : ApplicationMessage
    {
        public Guid ApplicationId { get; set; }
        public int ProcessId { get; set; }
    }
}
