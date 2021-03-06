using NodaTime;

namespace BrokerageApi.V1.Infrastructure.AuditEvents
{
    public class ReferralAssignmentAuditEventMetadata : AuditMetadataBase
    {
        public int ReferralId { get; set; }
        public string AssignedBrokerName { get; set; }
    }
    public class BudgetApproverAssignmentAuditEventMetadata : AuditMetadataBase
    {
        public int ReferralId { get; set; }
        public string AssignedApproverName { get; set; }
    }

    public class ElementAuditEventMetadata : AuditMetadataBase
    {
        public int ReferralId { get; set; }
        public int ElementId { get; set; }
        public string ElementDetails { get; set; }
        public string Comment { get; set; }
    }
    public class ReferralAuditEventMetadata : AuditMetadataBase
    {
        public int ReferralId { get; set; }
        public string Comment { get; set; }
    }
    public class ReferralFollowUpAuditEventMetadata : AuditMetadataBase
    {
        public int ReferralId { get; set; }
        public string Comment { get; set; }
        public LocalDate Date { get; set; }
    }
    public class CarePackageApprovalAuditEventMetadata : AuditMetadataBase
    {
        public int ReferralId { get; set; }
    }
    public class CareChargesConfirmedAuditEventMetadata : AuditMetadataBase
    {
        public int ReferralId { get; set; }
    }
}
