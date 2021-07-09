using System.Text.RegularExpressions;

namespace p15.Core.Services
{
    public class TextReplacementService
    {
        private readonly IDateTimeService _dateTimeService;

        public TextReplacementService(IDateTimeService dateTimeService)
        {
            _dateTimeService = dateTimeService;
        }

        public string ReplaceDates(string text)
        {
            var match = Regex.Match(text, @"(?<date>{today(?<offset>(?<direction>[+-])(?<distance>\d+))?(:(?<format>[^}]*))?})");
            if (match.Success)
            {
                var dt = _dateTimeService.Today;

                if (match.Groups["direction"].Success)
                {
                    var distance = int.Parse(match.Groups["distance"].Value);
                    var direction = match.Groups["direction"].Value;

                    if (direction == "-") distance = -distance;
                    dt = dt.AddDays(distance);
                }

                var strDate = dt.ToString(
                    match.Groups["format"].Success
                        ? match.Groups["format"].Value
                        : "d"
                );

                text = text.Replace(match.Groups["date"].Value, strDate);
            }
            return text;
        }
    }
}
