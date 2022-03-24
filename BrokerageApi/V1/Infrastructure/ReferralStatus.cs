using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace BrokerageApi.V1.Infrastructure
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ReferralStatus
    {
        Unassigned,
        InReview,
        Assigned,
        OnHold,
        Archived,
        InProgress,
        AwaitingApproval,
        Approved
    }
}
