using System.ComponentModel.DataAnnotations;
using BrokerageApi.V1.Infrastructure;
using NodaTime;

namespace BrokerageApi.V1.Boundary.Request
{
    public class EditElementRequest
    {
        public int ElementTypeId { get; set; }

        public bool NonPersonalBudget { get; set; }

        public int? ProviderId { get; set; }

        public string Details { get; set; }

        public ElementCost? Monday { get; set; }

        public ElementCost? Tuesday { get; set; }

        public ElementCost? Wednesday { get; set; }

        public ElementCost? Thursday { get; set; }

        public ElementCost? Friday { get; set; }

        public ElementCost? Saturday { get; set; }

        public ElementCost? Sunday { get; set; }

        public decimal? Quantity { get; set; }

        public decimal Cost { get; set; }

        public LocalDate StartDate { get; set; }

        public LocalDate? EndDate { get; set; }
    }
}
