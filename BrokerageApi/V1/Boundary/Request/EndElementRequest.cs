using NodaTime;

namespace BrokerageApi.V1.Boundary.Request
{
    public class EndElementRequest
    {
        public LocalDate EndDate { get; set; }
    }
}
