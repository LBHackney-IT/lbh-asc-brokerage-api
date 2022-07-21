using System.Collections.Generic;
using Newtonsoft.Json;
using NodaTime;
using BrokerageApi.V1.Infrastructure;

namespace BrokerageApi.V1.Boundary.Response
{
    public class ServiceOverviewResponse
    {
        public int Id { get; set; }

        [JsonProperty(Required = Required.DisallowNull)]
        public string Name { get; set; }

        public LocalDate StartDate { get; set; }

        public LocalDate? EndDate { get; set; }

        public decimal? WeeklyCost { get; set; }

        public decimal? WeeklyPayment { get; set; }

        public decimal? AnnualCost { get; set; }

        public ServiceStatus Status { get; set; }

        public List<ServiceOverviewElementResponse> Elements { get; set; }

        public bool ShouldSerializeElements() => Elements != null;
    }
}
