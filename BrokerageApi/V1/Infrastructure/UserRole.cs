using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace BrokerageApi.V1.Infrastructure
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum UserRole
    {
        BrokerageAssistant,
        Broker,
        Approver,
        CareChargesOfficer,
        Referrer
    }
}
