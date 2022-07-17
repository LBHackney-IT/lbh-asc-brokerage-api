using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using NodaTime;

namespace BrokerageApi.V1.Infrastructure
{
    public class CarePackage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string WorkflowId { get; set; }

        public WorkflowType WorkflowType { get; set; }

        public List<Workflow> Workflows { get; set; }

        [Required]
        public string FormName { get; set; }

        [Required]
        public string SocialCareId { get; set; }

        [Required]
        public string ResidentName { get; set; }

        public string PrimarySupportReason { get; set; }

        public Instant? UrgentSince { get; set; }

        public string CarePackageName { get; set; }

        public string AssignedBrokerId { get; set; }

        public User AssignedBroker { get; set; }

        public string AssignedApproverId { get; set; }

        public User AssignedApprover { get; set; }

        public ReferralStatus Status { get; set; }

        public string Note { get; set; }

        public Instant? StartedAt { get; set; }

        public bool IsResidential { get; set; }

        public CareChargeStatus CareChargeStatus { get; set; }

        public Instant? CareChargesConfirmedAt { get; set; }

        public Instant CreatedAt { get; set; }

        public Instant UpdatedAt { get; set; }

        public LocalDate? StartDate { get; set; }

        public decimal? WeeklyCost { get; set; }

        public decimal? WeeklyPayment { get; set; }

        public decimal? OneOffPayment { get; set; }

        public List<ReferralElement> ReferralElements { get; set; }

        public List<Element> Elements { get; set; }

        [CanBeNull]
        public List<ReferralAmendment> ReferralAmendments { get; set; }

        public string Comment { get; set; }
        public decimal EstimatedYearlyCost { get; set; }
    }
}
