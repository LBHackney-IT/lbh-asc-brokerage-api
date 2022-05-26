using System.Collections.Generic;
using Newtonsoft.Json;
using NodaTime;
using BrokerageApi.V1.Infrastructure;

namespace BrokerageApi.V1.Boundary.Response
{
    public class ElementResponse
    {
        public int Id { get; set; }

        public ElementTypeResponse ElementType { get; set; }

        public bool NonPersonalBudget { get; set; }

        public ProviderResponse Provider { get; set; }

        [JsonProperty(Required = Required.DisallowNull)]
        public string Details { get; set; }

        public ElementStatus Status { get; set; }

        public ElementResponse ParentElement { get; set; }
        public List<ElementResponse> SuspensionElements { get; set; }

        public LocalDate StartDate { get; set; }

        public LocalDate? EndDate { get; set; }

        public ElementCost? Monday { get; set; }

        public ElementCost? Tuesday { get; set; }

        public ElementCost? Wednesday { get; set; }

        public ElementCost? Thursday { get; set; }

        public ElementCost? Friday { get; set; }

        public ElementCost? Saturday { get; set; }

        public ElementCost? Sunday { get; set; }

        public decimal? Quantity { get; set; }

        public decimal Cost { get; set; }

        public Instant CreatedAt { get; set; }

        public Instant UpdatedAt { get; set; }
    }
}
