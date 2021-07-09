using System;

namespace p15.Core.Messages
{
    public class PrepareApplicationMessage
    {
        public Guid ApplicationId { get; set; }
        public string ApplicationName { get; set; }
        public string ProjectName { get; set; }
    }
}
