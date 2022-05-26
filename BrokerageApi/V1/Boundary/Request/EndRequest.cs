using NodaTime;

namespace BrokerageApi.V1.Boundary.Request
{
    public class EndRequest
    {
        public LocalDate EndDate { get; set; }
    }
}
