using System;

namespace p15.Core.Services
{
    public interface IDateTimeService
    {
        DateTime Today { get; }
    }

    public class DateTimeService : IDateTimeService
    {
        public DateTime Today => DateTime.Today;
    }
}
