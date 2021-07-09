using p15.Core.Models;
using System;
using System.Text.RegularExpressions;

namespace p15.Core.Parsers
{
    public class AdbLogParser
    {
        private const string _logHeaderPattern = @"\[\s+(?<month>\d{2})-(?<day>\d{2})\s+(?<hour>\d{2}):(?<minute>\d{2}):(?<second>\d{2}).(?<millisecond>\d{3})\s+\d+:\s*\d+\s+(?<severity>\D)/(?<tag>)\S+\s+\]";
        private LogEntryModel _cache;

        public LogEntryModel Parse(string text)
        {
            if (text.Length == 0)
            {
                if (_cache != null)
                {
                    var result = _cache;
                    _cache = null;
                    return result;
                }
            }
            else
            {
                var match = Regex.Match(text, _logHeaderPattern);
                if (match.Success)
                {
                    _cache = new LogEntryModel
                    {
                        Timestamp = new DateTime(
                            DateTime.Now.Year,
                            int.Parse(match.Groups["month"].Value),
                            int.Parse(match.Groups["day"].Value),
                            int.Parse(match.Groups["hour"].Value),
                            int.Parse(match.Groups["minute"].Value),
                            int.Parse(match.Groups["second"].Value),
                            int.Parse(match.Groups["millisecond"].Value)
                        ),
                        Severity = MapCharacterToSeverity(match.Groups["severity"].Value[0]),
                        Message = ""
                    };
                }
                else
                {
                    if (_cache != null)
                    {
                        _cache.Message += text + Environment.NewLine;
                    }
                }
            }
            return null;
        }

        private string MapCharacterToSeverity(char severity) => char.ToLower(severity) switch
        {
            'e' => "Error",
            'd' => "Debug",
            'w' => "Warning",
            'f' => "Fatal",
            'i' => "Information",
            'v' => "Verbose",
            _ => ""
        };
    }
}
