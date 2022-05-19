using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BrokerageApi.V1.Infrastructure.AuditEvents
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum AuditEventType
    {
        ReferralBrokerAssignment,
        ReferralBrokerReassignment,
        ElementEnded,
        ElementCancelled,
        ElementSuspended,
        CarePackageEnded,
        CarePackageCancelled,
        CarePackageSuspended
    }
}
