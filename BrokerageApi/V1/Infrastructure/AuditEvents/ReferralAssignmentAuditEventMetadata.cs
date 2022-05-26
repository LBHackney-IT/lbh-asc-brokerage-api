namespace BrokerageApi.V1.Infrastructure.AuditEvents
{
    public class ReferralAssignmentAuditEventMetadata : AuditMetadataBase
    {
        public string AssignedBrokerName { get; set; }
    }
    public class ReferralReassignmentAuditEventMetadata : AuditMetadataBase
    {
        public string AssignedBrokerName { get; set; }
    }
    public class ElementAuditEventMetadata : AuditMetadataBase
    {
        public int ReferralId { get; set; }
        public int ElementId { get; set; }
        public string ElementDetails { get; set; }
        public string Comment { get; set; }
    }
    public class CarePackageAuditEventMetadata : AuditMetadataBase
    {
        public int ReferralId { get; set; }
        public string Comment { get; set; }
    }
}
