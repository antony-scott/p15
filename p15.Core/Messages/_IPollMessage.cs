using System;

namespace p15.Core.Messages
{
    public interface IPollMessage
    {
        TimeSpan Interval { get; }
    }
}
