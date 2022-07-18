using System.Collections.Generic;
using BrokerageApi.V1.Boundary.Response.Interfaces;
using Newtonsoft.Json;
using BrokerageApi.V1.Infrastructure;
using NodaTime;

namespace BrokerageApi.V1.Boundary.Response
{
    public class WorkflowResponse
    {
        [JsonProperty(Required = Required.DisallowNull)]
        public string Id { get; set; }

        public WorkflowType WorkflowType { get; set; }

        [JsonProperty(Required = Required.DisallowNull)]
        public string FormName { get; set; }

        public string Note { get; set; }

        public string PrimarySupportReason { get; set; }

        public string DirectPayments { get; set; }

        public Instant? UrgentSince { get; set; }
    }
}
