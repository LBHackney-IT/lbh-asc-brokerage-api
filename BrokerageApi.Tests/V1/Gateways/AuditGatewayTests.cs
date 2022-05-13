using System.Threading.Tasks;
using BrokerageApi.Tests.V1.UseCase;
using BrokerageApi.V1.Gateways;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.Infrastructure.AuditEvents;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NUnit.Framework;

namespace BrokerageApi.Tests.V1.Gateways
{
    [TestFixture]
    public class AuditGatewayTests : DatabaseTests
    {
        private AuditGateway _classUnderTest;

        [SetUp]
        public void Setup()
        {
            _classUnderTest = new AuditGateway(BrokerageContext);
        }

        [TestCase(AuditEventType.ReferralBrokerAssignment, "Assigned to broker")]
        [TestCase(AuditEventType.ReferralBrokerReassignment, "Reassigned to broker")]
        public async Task CanAddAuditEvent(AuditEventType eventType, string expectedMessage)
        {
            var expectedUser = await SeedUser();
            var expectedType = eventType;
            var expectedSocialCareId = "socialCareId";

            await _classUnderTest.AddAuditEvent(expectedType, expectedSocialCareId, expectedUser.Id, null);

            var auditEvent = await BrokerageContext.AuditEvents.SingleOrDefaultAsync(ae => ae.EventType == expectedType);
            auditEvent.CreatedAt.Should().Be(CurrentInstant);
            auditEvent.SocialCareId.Should().Be(expectedSocialCareId);
            auditEvent.UserId.Should().Be(expectedUser.Id);
            auditEvent.Message.Should().Be(expectedMessage);
        }

        private static readonly object[] _metadataTests =
        {
            new object[]
            {
                AuditEventType.ReferralBrokerAssignment, "Assigned to broker", new ReferralAssignmentAuditEventMetadata
                {
                    AssignedBrokerName = "TestBroker"
                }
            },
            new object[]
            {
                AuditEventType.ReferralBrokerReassignment, "Reassigned to broker", new ReferralReassignmentAuditEventMetadata
                {
                    AssignedBrokerName = "TestBroker"
                }
            },
        };

        [TestCaseSource(nameof(_metadataTests))]
        public async Task ReferralBrokerAssignmentHasCorrectMetadata(AuditEventType eventType, string expectedMessage, AuditMetadataBase metadata)
        {
            var expectedUser = await SeedUser();
            const string expectedSocialCareId = "socialCareId";

            await _classUnderTest.AddAuditEvent(eventType, "socialCareId", expectedUser.Id, metadata);

            var auditEvent = await BrokerageContext.AuditEvents.SingleOrDefaultAsync(ae => ae.EventType == eventType);
            auditEvent.Metadata.Should().Be(JsonConvert.SerializeObject(metadata));
            auditEvent.CreatedAt.Should().Be(CurrentInstant);
            auditEvent.SocialCareId.Should().Be(expectedSocialCareId);
            auditEvent.UserId.Should().Be(expectedUser.Id);
            auditEvent.Message.Should().Be(expectedMessage);
        }

        private async Task<User> SeedUser()
        {

            var expectedUser = new User
            {
                Name = "test",
                Email = "test@email.com"
            };
            await BrokerageContext.Users.AddAsync(expectedUser);
            await BrokerageContext.SaveChangesAsync();
            return expectedUser;
        }
    }
}
