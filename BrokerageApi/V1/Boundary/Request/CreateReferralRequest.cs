using System;
using System.ComponentModel.DataAnnotations;
using BrokerageApi.V1.Infrastructure;

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

        public DateTime? UrgentSince { get; set; }

        public string Note { get; set; }
    }
}
