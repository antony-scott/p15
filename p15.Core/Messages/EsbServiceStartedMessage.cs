using System;

namespace p15.Core.Messages
{
    public class EsbServiceStartedMessage : ApplicationStartedMessage
    {
        public string Filename { get; set; }
        public string Arguments { get; set; }
    }
}
