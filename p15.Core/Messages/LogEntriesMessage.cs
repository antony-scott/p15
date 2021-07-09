using p15.Core.Models;
using System;
using System.Collections.Generic;

namespace p15.Core.Messages
{
    public class LogEntriesMessage
    {
        public string Name { get; set; }
        public Guid Identifier { get; set; }
        public IEnumerable<LogEntryModel> LogEntries { get; set; }
    }
}
