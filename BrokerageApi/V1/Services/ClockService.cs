using NodaTime;
using BrokerageApi.V1.Services.Interfaces;

namespace BrokerageApi.V1.Services
{
    public class ClockService : IClockService
    {
        private readonly IClock _clock;

        public ClockService() : this(SystemClock.Instance)
        {
        }

        public ClockService(IClock clock)
        {
            _clock = clock;
        }

        public Instant Now
        {
            get => _clock.GetCurrentInstant();
        }

        public DateTimeZone TimeZone
        {
            get => DateTimeZoneProviders.Tzdb["Europe/London"];
        }

        public LocalDate Today
        {
            get => Now.InZone(TimeZone).Date;
        }
    }
}
