using NodaTime;

namespace BrokerageApi.V1.Services.Interfaces
{
    public interface IClockService
    {
        public Instant Now { get; }
    }
}
