using System.ComponentModel.DataAnnotations;

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
        public string SocialCareId { get; set; }

        [Required]
        public string Name { get; set; }

        public string AssignedTo { get; set; }

        public ReferralStatus Status { get; set; }
    }
}
