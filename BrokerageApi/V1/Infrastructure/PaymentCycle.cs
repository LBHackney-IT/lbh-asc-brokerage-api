using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BrokerageApi.V1.Infrastructure
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PaymentCycle
    {
        Weekly,
        Fortnightly,
        FourWeekly,
        Varying,
        Once
    }
}
