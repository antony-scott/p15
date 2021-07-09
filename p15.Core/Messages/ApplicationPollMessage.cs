using System;

namespace p15.Core.Messages
{
    public class ApplicationPollMessage : IPollMessage
    {
        public TimeSpan Interval => TimeSpan.FromSeconds(15);
    }
}