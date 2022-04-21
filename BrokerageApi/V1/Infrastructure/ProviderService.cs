using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BrokerageApi.V1.Infrastructure
{
    public class ProviderService
    {
        public int ProviderId { get; set; }
        public Provider Provider { get; set; }

        public int ServiceId { get; set; }
        public Service Service { get; set; }

        [Required]
        public string SubjectiveCode { get; set; }
    }
}
