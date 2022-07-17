using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BrokerageApi.V1.Infrastructure
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum CareChargeStatus
    {
        New,
        Existing,
        Termination,
        Suspension,
        Cancellation
    }
}
