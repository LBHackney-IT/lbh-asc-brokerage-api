using NodaTime;

namespace BrokerageApi.V1.Boundary.Request
{
    public class SuspendRequest : ICommentRequest
    {
        public LocalDate StartDate { get; set; }
        public LocalDate? EndDate { get; set; }
        public string Comment { get; set; }
    }
}
