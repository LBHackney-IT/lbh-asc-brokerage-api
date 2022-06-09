using NodaTime;

namespace BrokerageApi.V1.Boundary.Request
{
    public class EndRequest : ICommentRequest
    {
        public LocalDate EndDate { get; set; }
        public string Comment { get; set; }
    }
}
