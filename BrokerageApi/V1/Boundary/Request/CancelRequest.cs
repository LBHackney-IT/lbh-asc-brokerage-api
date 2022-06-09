namespace BrokerageApi.V1.Boundary.Request
{
    public class CancelRequest : ICommentRequest
    {
        public string Comment { get; set; }
    }
}
