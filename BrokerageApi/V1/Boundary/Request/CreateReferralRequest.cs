using System.ComponentModel.DataAnnotations;
using NodaTime;
using BrokerageApi.V1.Infrastructure;
using System.Collections.Generic;

namespace BrokerageApi.V1.Boundary.Request
{
    public class CreateReferralRequest
    {
        [Required]
        public string WorkflowId { get; set; }

        [Required]
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

        public string Note { get; set; }
    }
}
