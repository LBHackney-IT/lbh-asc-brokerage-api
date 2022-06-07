using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NpgsqlTypes;

namespace BrokerageApi.V1.Infrastructure
{
    public class Provider : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Address { get; set; }

        public ProviderType Type { get; set; }

        public string CedarNumber { get; set; }

        public bool IsArchived { get; set; }

        public NpgsqlTsVector SearchVector { get; set; }

        public List<ProviderService> ProviderServices { get; set; }

        public List<Service> Services { get; set; }

        public List<Element> Elements { get; set; }
    }
}
