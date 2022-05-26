using NodaTime;

namespace BrokerageApi.V1.Boundary.Request
{
    public class SuspendRequest
    {
        public LocalDate StartDate { get; set; }
        public LocalDate EndDate { get; set; }
    }
}
