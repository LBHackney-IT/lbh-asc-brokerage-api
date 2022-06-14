using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure.AuditEvents;
using Moq;

namespace BrokerageApi.Tests.V1.UseCase.Mocks
{
    public class MockAuditGateway : Mock<IAuditGateway>
    {
        public int LastUserId => AllCalls.LastOrDefault().userId;
        public string LastSocialCareId => AllCalls.LastOrDefault().socialCareId;
        public AuditMetadataBase LastMetadata => AllCalls.LastOrDefault().metadata;

        public Queue<(int userId, string socialCareId, AuditMetadataBase metadata, AuditEventType type)> AllCalls { get; } = new Queue<(int userId, string socialCareId, AuditMetadataBase metadata, AuditEventType type)>();

        public MockAuditGateway()
        {
            Setup(x => x.AddAuditEvent(It.IsAny<AuditEventType>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<AuditMetadataBase>()))
                .Callback<AuditEventType, string, int, AuditMetadataBase>((type, socialCareId, userId, metadata) =>
                {
                    AllCalls.Enqueue((userId, socialCareId, metadata, type));
                })
                .Returns(Task.CompletedTask);
        }
        public void VerifyAuditEventAdded(AuditEventType eventType)
        {
            Verify(x => x.AddAuditEvent(eventType, It.IsAny<string>(), It.IsAny<int>(), It.IsAny<AuditMetadataBase>()));
        }
    }
}
