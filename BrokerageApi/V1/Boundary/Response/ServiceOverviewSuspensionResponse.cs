using System.Collections.Generic;
using NodaTime;
using BrokerageApi.V1.Infrastructure;
using JetBrains.Annotations;

namespace BrokerageApi.V1.Boundary.Response
{
    public class ServiceOverviewSuspensionResponse
    {
        public int Id { get; set; }

        public ReferralResponse Referral { get; set; }

        public LocalDate StartDate { get; set; }

        public LocalDate? EndDate { get; set; }

        public ElementStatus Status { get; set; }

        public decimal? Quantity { get; set; }

        public decimal Cost { get; set; }
    }
}
