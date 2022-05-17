using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NodaTime;

namespace BrokerageApi.V1.Infrastructure
{
    public class Referral : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string WorkflowId { get; set; }

        public WorkflowType WorkflowType { get; set; }

        [Required]
        public string FormName { get; set; }

        [Required]
        public string SocialCareId { get; set; }

        [Required]
        public string ResidentName { get; set; }

        public string PrimarySupportReason { get; set; }

        public string DirectPayments { get; set; }

        public Instant? UrgentSince { get; set; }

        public string AssignedTo { get; set; }

        public ReferralStatus Status { get; set; }

        public string Note { get; set; }

        public Instant? StartedAt { get; set; }

        public List<ReferralElement> ReferralElements { get; set; }

        public List<Element> Elements { get; set; }
    }
}
