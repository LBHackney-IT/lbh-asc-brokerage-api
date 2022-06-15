using BrokerageApi.V1.Infrastructure;

namespace BrokerageApi.V1.Boundary.Response
{
    public class AmendmentResponse
    {
        public AmendmentStatus Status { get; set; }
        public string Comment { get; set; }
    }
}
