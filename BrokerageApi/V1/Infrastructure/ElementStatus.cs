using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BrokerageApi.V1.Infrastructure
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ElementStatus
    {
        InProgress,
        AwaitingApproval,
        Approved,
        Inactive,
        Active,
        Ended,
        Suspended,
        Cancelled
    }
}
