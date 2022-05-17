using System.Threading.Tasks;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure.AuditEvents;
using Moq;

namespace BrokerageApi.Tests.V1.UseCase.Mocks
{
    public class MockAuditGateway : Mock<IAuditGateway>
    {
        public int LastUserId { get; set; }
        public string LastSocialCareId { get; set; }
        public AuditMetadataBase LastMetadata { get; set; }

        public MockAuditGateway()
        {
            Setup(x => x.AddAuditEvent(It.IsAny<AuditEventType>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<AuditMetadataBase>()))
                .Callback<AuditEventType, string, int, AuditMetadataBase>((type, socialCareId, userId, metadata) =>
                {
                    LastSocialCareId = socialCareId;
                    LastUserId = userId;
                    LastMetadata = metadata;
                })
                .Returns(Task.CompletedTask);
        }
        public void VerifyAuditEventAdded(AuditEventType eventType)
        {
            Verify(x => x.AddAuditEvent(eventType, It.IsAny<string>(), It.IsAny<int>(), It.IsAny<AuditMetadataBase>()));
        }
    }
}
