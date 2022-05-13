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
}
