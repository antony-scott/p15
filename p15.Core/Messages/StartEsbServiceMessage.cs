using System;
using System.Collections.Generic;

namespace p15.Core.Messages
{
    public class StartEsbServiceMessage
    {
        public Guid ApplicationId { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public IEnumerable<string> Arguments { get; set; }
    }
}
