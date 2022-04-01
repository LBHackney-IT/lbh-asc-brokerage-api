using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using BrokerageApi.V1.Infrastructure;

namespace BrokerageApi.V1.Boundary.Response
{
    public class ReferralResponse
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

        public DateTime? UrgentSince { get; set; }

        public string AssignedTo { get; set; }

        public ReferralStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
