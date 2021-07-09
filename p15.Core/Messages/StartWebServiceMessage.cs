using System;

namespace p15.Core.Messages
{
    public class StartWebServiceMessage
    {
        public Guid ApplicationId { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
    }
}
