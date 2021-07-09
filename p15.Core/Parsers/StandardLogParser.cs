using p15.Core.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace p15.Core.Parsers
{
    public class StandardLogParser
    {
        public IEnumerable<LogEntryModel> Parse(string text, int? lastNEntries)
        {
            var lines = Regex
                .Split(text, "\u001e")
                .Where(line => line.Length > 0)
                .ToList();

            if (lastNEntries.HasValue)
            {
                lines.RemoveRange(0, Math.Max(0, lines.Count - lastNEntries.Value));
            }

            var entries = lines
                .Where(line => line.Length > 0)
                .Select(line =>
                {
                    var parts = line.Split('\t');

                    if (parts.Length == 1)
                    {
                        return new LogEntryModel
                        {
                            Message = parts[0]
                        };
                    }

                    var strThreadId = Regex.Split(parts[1], @"\D").First();
                    var timeStamp = DateTime.TryParse(parts[0], null, DateTimeStyles.None, out var ts) ? ts : (DateTime?)null;
                    var threadId = int.Parse(strThreadId);
                    var severity = parts[2];

                    return new LogEntryModel
                    {
                        Severity = severity,
                        Message = string.Join("\t", parts.Skip(3)),
                        Timestamp = timeStamp
                    };
                });

            return entries;
        }
    }
}
