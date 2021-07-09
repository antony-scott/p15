using System;

namespace p15.Core.Messages
{
    public class ProcessMessageQueuesMessage : BootstrapMessage
    {
        public Guid ApplicationId { get; set; }
        public string Path { get; set; }
    }
}
