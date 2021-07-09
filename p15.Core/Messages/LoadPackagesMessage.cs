using System;

namespace p15.Core.Messages
{
    public class LoadPackagesMessage : IPollMessage
    {
        public TimeSpan Interval => TimeSpan.FromSeconds(1);
    }
}
