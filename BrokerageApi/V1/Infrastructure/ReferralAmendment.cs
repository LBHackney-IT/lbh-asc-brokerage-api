using System.Collections.Generic;

namespace BrokerageApi.V1.Infrastructure
{
    public class ReferralAmendment
    {
        public int Id { get; set; }

        public int ReferralId { get; set; }

        public Referral Referral { get; set; }

        public string Comment { get; set; }

        public AmendmentStatus Status { get; set; }
    }

    public enum AmendmentStatus
    {
        InProgress,
        Resolved
    }
}
