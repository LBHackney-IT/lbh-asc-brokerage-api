using System;
using BrokerageApi.V1.Infrastructure;

namespace BrokerageApi.V1.Boundary.Response
{
    public class ReferralResponse
    {
        public int Id { get; set; }

        public string WorkflowId { get; set; }

        public WorkflowType WorkflowType { get; set; }

        public string SocialCareId { get; set; }

        public string Name { get; set; }

        public DateTime? UrgentSince { get; set; }

        public string AssignedTo { get; set; }

        public ReferralStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
