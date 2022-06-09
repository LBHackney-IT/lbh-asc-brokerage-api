using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BrokerageApi.V1.Infrastructure
{
    public class ElementType
    {
        [Key]
        public int Id { get; set; }

        public int ServiceId { get; set; }

        [Required]
        public string Name { get; set; }

        public string SubjectiveCode { get; set; }

        public ElementTypeType Type { get; set; }

        public ElementCostType CostType { get; set; }

        public ElementBillingType Billing { get; set; }

        public bool NonPersonalBudget { get; set; }

        public int Position { get; set; }

        public bool IsArchived { get; set; }

        public Service Service { get; set; }

        public List<Element> Elements { get; set; }
    }
}
