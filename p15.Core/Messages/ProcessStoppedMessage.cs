using System;

namespace p15.Core.Messages
{
    public class ProcessStoppedMessage : ApplicationMessage
    {
        public Guid ApplicationId { get; set; }
    }
}
