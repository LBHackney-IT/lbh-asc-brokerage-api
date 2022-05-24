using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

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
        Approved,
        Active,
        Ended
    }
}
