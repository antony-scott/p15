using System;

namespace p15.Core.Models
{
    public class LogEntryModel
    {
        public DateTime? Timestamp { get; set; }
        public string Severity { get; set; }
        public string Message { get; set; }
    }
}
