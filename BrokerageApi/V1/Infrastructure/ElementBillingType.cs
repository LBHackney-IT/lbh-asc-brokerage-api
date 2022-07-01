using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BrokerageApi.V1.Infrastructure
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ElementBillingType
    {
        Supplier,
        Customer,
        None,
        Ccg
    }
}
