using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BrokerageApi.V1.Infrastructure
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ElementTypeType
    {
        Service,
        ProvisionalCareCharge,
        ConfirmedCareCharge,
        NursingCare
    }
}
