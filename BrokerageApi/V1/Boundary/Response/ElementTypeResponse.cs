using Newtonsoft.Json;
using BrokerageApi.V1.Infrastructure;

namespace BrokerageApi.V1.Boundary.Response
{
    public class ElementTypeResponse
    {
        public int Id { get; set; }

        [JsonProperty(Required = Required.DisallowNull)]
        public string Name { get; set; }

        public ElementTypeType Type { get; set; }

        public ElementCostType CostType { get; set; }

        public ElementBillingType Billing { get; set; }

        public bool NonPersonalBudget { get; set; }

        public bool IsS117 { get; set; }

        public ServiceResponse Service { get; set; }

        public bool ShouldSerializeService() => Service != null;
    }
}
