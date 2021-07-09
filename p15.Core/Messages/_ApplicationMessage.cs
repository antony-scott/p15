using System;

namespace p15.Core.Messages
{
    public interface ApplicationMessage
    {
        Guid ApplicationId { get; }
    }
}
