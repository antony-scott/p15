using System;

namespace p15.Core.Messages
{
    public class ApplicationIsRunningMessage : ApplicationMessage
    {
        public Guid ApplicationId { get; set; }
        public int ProcessId { get; set; }
    }
}
