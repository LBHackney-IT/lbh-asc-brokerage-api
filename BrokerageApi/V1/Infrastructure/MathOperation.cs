using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BrokerageApi.V1.Infrastructure
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum MathOperation
    {
        Subtract = -1,
        Ignore = 0,
        Add = 1
    }
}
