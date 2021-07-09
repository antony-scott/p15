using System;

namespace p15.Core.Messages
{
    public class WebServiceStartedMessage : ApplicationStartedMessage
    {
        public int Port { get; set; }
    }
}
