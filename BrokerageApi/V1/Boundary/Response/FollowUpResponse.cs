using BrokerageApi.V1.Infrastructure;
using NodaTime;

namespace BrokerageApi.V1.Boundary.Response
{
    public class FollowUpResponse
    {
        public FollowUpStatus Status { get; set; }
        public string Comment { get; set; }
        public LocalDate Date { get; set; }
        public Instant RequestedAt { get; set; }
        public UserResponse RequestedBy { get; set; }
    }
}
