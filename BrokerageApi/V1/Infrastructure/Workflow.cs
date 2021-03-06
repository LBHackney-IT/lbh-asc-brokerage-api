using Newtonsoft.Json;
using NodaTime;
using System.ComponentModel.DataAnnotations;

namespace BrokerageApi.V1.Infrastructure
{
    public class Workflow : BaseEntity
    {
        [Key]
        public string Id { get; set; }
        public int ReferralId { get; set; }
        [Required]
        public string FormName { get; set; }
        public WorkflowType WorkflowType { get; set; }
        public string Note { get; set; }
        public string PrimarySupportReason { get; set; }
        public string DirectPayments { get; set; }
        public Instant? UrgentSince { get; set; }
        [JsonIgnore]
        public Referral Referral { get; set; }
    }
}
