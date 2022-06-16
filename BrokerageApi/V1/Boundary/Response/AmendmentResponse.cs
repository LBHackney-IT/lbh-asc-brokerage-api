using BrokerageApi.V1.Infrastructure;
using NodaTime;

namespace BrokerageApi.V1.Boundary.Response
{
    public class AmendmentResponse
    {
        public AmendmentStatus Status { get; set; }
        public string Comment { get; set; }
        public Instant RequestedAt { get; set; }
    }
}
