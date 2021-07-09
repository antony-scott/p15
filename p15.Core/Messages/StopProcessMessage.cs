using System;

namespace p15.Core.Messages
{
    public class StopProcessMessage
    {
        public Guid ApplicationId { get; set; }
        public int ProcessId { get; set; }
    }
}
