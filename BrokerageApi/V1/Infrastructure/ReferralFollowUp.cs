using NodaTime;

namespace BrokerageApi.V1.Infrastructure
{
    public class ReferralFollowUp
    {
        public int Id { get; set; }

        public int ReferralId { get; set; }

        public Referral Referral { get; set; }

        public string Comment { get; set; }

        public LocalDate Date { get; set; }

        public FollowUpStatus Status { get; set; }

        public Instant RequestedAt { get; set; }

        public string RequestedByEmail { get; set; }
        public User RequestedBy { get; set; }
    }

    public enum FollowUpStatus
    {
        InProgress,
        Resolved
    }
}
