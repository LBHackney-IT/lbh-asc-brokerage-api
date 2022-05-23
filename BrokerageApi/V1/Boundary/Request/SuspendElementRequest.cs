using NodaTime;

namespace BrokerageApi.V1.Boundary.Request
{
    public class SuspendElementRequest
    {
        public LocalDate StartDate { get; set; }
        public LocalDate EndDate { get; set; }
    }
}
