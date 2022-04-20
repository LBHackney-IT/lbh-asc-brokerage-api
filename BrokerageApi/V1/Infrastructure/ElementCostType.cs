using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace BrokerageApi.V1.Infrastructure
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ElementCostType
    {
        Hourly,
        Daily,
        Weekly,
        Transport,
        OneOff
    }
}
