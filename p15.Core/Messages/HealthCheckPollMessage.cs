using System;

namespace p15.Core.Messages
{
    public class HealthCheckPollMessage : IPollMessage
    {
        public TimeSpan Interval => TimeSpan.FromMinutes(1);
    }
}
