using JetBrains.Annotations;
using NodaTime;

namespace BrokerageApi.V1.Infrastructure
{
    public class ReferralElement
    {
        public int ReferralId { get; set; }
        public Referral Referral { get; set; }
        public CarePackage CarePackage { get; set; }

        public int ElementId { get; set; }
        public Element Element { get; set; }
        public LocalDate?
        PendingEndDate
        { get; set; }
        public bool PendingCancellation { get; set; }
        [CanBeNull]
        public string PendingComment { get; set; }
    }
}
