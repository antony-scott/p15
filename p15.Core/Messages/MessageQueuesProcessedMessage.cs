using System;

namespace p15.Core.Messages
{
    public class MessageQueuesProcessedMessage : BootstrapMessage
    {
        public Guid ApplicationId { get; set; }
    }
}
