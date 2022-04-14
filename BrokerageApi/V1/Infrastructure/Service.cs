using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BrokerageApi.V1.Infrastructure
{
    public class Service
    {
        [Key]
        public int Id { get; set; }

        public int? ParentId { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        public int Position { get; set; }

        public bool IsArchived { get; set; }

        public Service Parent { get; set; }

        public List<Service> Services { get; set; }
    }
}
