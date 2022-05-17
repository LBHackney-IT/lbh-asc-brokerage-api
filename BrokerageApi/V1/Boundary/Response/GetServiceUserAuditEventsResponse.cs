using System.Collections.Generic;

namespace BrokerageApi.V1.Boundary.Response
{
    public class GetServiceUserAuditEventsResponse
    {
        public PageMetadataResponse PageMetadata { get; set; }
        public List<AuditEventResponse> Events { get; set; }
    }

}
