using System;

namespace p15.Core.Messages
{
    public class ApplicationSyncedMessage : BootstrapMessage
    {
        public Guid ApplicationId { get; internal set; }
    }
}
