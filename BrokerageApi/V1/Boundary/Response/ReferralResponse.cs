using BrokerageApi.V1.Boundary.Response.Interfaces;
using Newtonsoft.Json;
using NodaTime;
using BrokerageApi.V1.Infrastructure;

namespace BrokerageApi.V1.Boundary.Response
{
    public class ReferralResponse : IAssigmentResponse
    {
        public int Id { get; set; }

        [JsonProperty(Required = Required.DisallowNull)]
        public string WorkflowId { get; set; }

        public WorkflowType WorkflowType { get; set; }

        [JsonProperty(Required = Required.DisallowNull)]
        public string FormName { get; set; }

        [JsonProperty(Required = Required.DisallowNull)]
        public string SocialCareId { get; set; }

        [JsonProperty(Required = Required.DisallowNull)]
        public string ResidentName { get; set; }

        public string PrimarySupportReason { get; set; }

        public string DirectPayments { get; set; }

        public Instant? UrgentSince { get; set; }

        public UserResponse AssignedBroker { get; set; }

        public UserResponse AssignedApprover { get; set; }

        public string AssignedTo => Status == ReferralStatus.AwaitingApproval ? AssignedApprover?.Email : AssignedBroker?.Email;

        public ReferralStatus Status { get; set; }

        public string Note { get; set; }

        public Instant? StartedAt { get; set; }

        public Instant CreatedAt { get; set; }

        public Instant UpdatedAt { get; set; }
    }
}
