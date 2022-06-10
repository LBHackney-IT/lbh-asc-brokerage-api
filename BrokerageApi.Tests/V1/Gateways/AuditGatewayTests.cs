using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using BrokerageApi.Tests.V1.Helpers;
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
        private Fixture _fixture;
        private AuditGateway _classUnderTest;

        [SetUp]
        public void Setup()
        {
            _fixture = FixtureHelpers.Fixture;
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
                    ReferralId = 1234,
                    AssignedBrokerName = "TestBroker"
                }
            },
            new object[]
            {
                AuditEventType.ReferralBrokerReassignment, "Reassigned to broker", new ReferralReassignmentAuditEventMetadata
                {
                    ReferralId = 1234,
                    AssignedBrokerName = "TestBroker"
                }
            },
        };


        [TestCaseSource(nameof(_metadataTests))]
        public async Task ReferralBrokerAssignmentHasCorrectMetadata(AuditEventType eventType, string expectedMessage, AuditMetadataBase metadata)
        {
            var expectedReferral = await SeedReferral();
            var expectedUser = await SeedUser();
            const string expectedSocialCareId = "socialCareId";

            await _classUnderTest.AddAuditEvent(eventType, "socialCareId", expectedUser.Id, metadata);

            var auditEvent = await BrokerageContext.AuditEvents
                .Include(ae => ae.Referral)
                .SingleOrDefaultAsync(ae => ae.EventType == eventType);

            auditEvent.Metadata.Should().Be(JsonConvert.SerializeObject(metadata));
            auditEvent.CreatedAt.Should().Be(CurrentInstant);
            auditEvent.SocialCareId.Should().Be(expectedSocialCareId);
            auditEvent.UserId.Should().Be(expectedUser.Id);
            auditEvent.Message.Should().Be(expectedMessage);
            auditEvent.Referral.Should().BeEquivalentTo(expectedReferral);
        }

        [Test]
        public async Task CanGetFilteredAuditEvents()
        {
            var expectedUser = await SeedUser();
            const string expectedSocialCareId = "socialCareId";

            var expectedEvents = await SeedEvents(expectedUser.Id, expectedSocialCareId);
            var unexpectedEvents = await SeedEvents(expectedUser.Id, _fixture.Create<string>());

            var events = _classUnderTest.GetServiceUserAuditEvents(expectedSocialCareId, 1, 100);

            events.Should().HaveCount(expectedEvents.Count());
            events.Should().Contain(expectedEvents);
            events.Should().NotContain(unexpectedEvents);
        }

        [Test]
        public async Task CanGetPagedResults([Range(1, 10)] int pageNumber)
        {
            var expectedUser = await SeedUser();
            const string expectedSocialCareId = "socialCareId";
            const int pageSize = 10;

            var allEvents = await SeedEvents(expectedUser.Id, expectedSocialCareId, 100);
            var expectedEvents = allEvents
                .OrderBy(e => e.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            var events = _classUnderTest.GetServiceUserAuditEvents(expectedSocialCareId, pageNumber, pageSize);

            events.Should().HaveCount(pageSize);
            events.Should().Contain(expectedEvents);

            var pageMetadata = events.GetMetaData();
            pageMetadata.PageNumber.Should().Be(pageNumber);
            pageMetadata.PageSize.Should().Be(pageSize);
            pageMetadata.PageCount.Should().Be((int) Math.Ceiling((float) allEvents.Count() / pageSize));
        }

        private async Task<IEnumerable<AuditEvent>> SeedEvents(int expectedUserId, string expectedSocialCareId, int count = 5)
        {
            var auditEvents = _fixture.Build<AuditEvent>()
                .With(ae => ae.UserId, expectedUserId)
                .With(ae => ae.SocialCareId, expectedSocialCareId)
                .Without(ae => ae.Metadata)
                .Without(ae => ae.Referral)
                .Without(ae => ae.User)
                .CreateMany(count);

            await BrokerageContext.AuditEvents.AddRangeAsync(auditEvents);
            await BrokerageContext.SaveChangesAsync();

            return auditEvents;
        }

        private async Task<Referral> SeedReferral()
        {
            var referral = new Referral()
            {
                Id = 1234,
                WorkflowId = "174079ae-75b4-43b4-9d29-363e88e7dd40",
                WorkflowType = WorkflowType.Assessment,
                FormName = "Care act assessment",
                SocialCareId = "33556688",
                ResidentName = "A Service User",
                PrimarySupportReason = "Physical Support",
                DirectPayments = "No",
                AssignedBroker = "a.broker@hackney.gov.uk",
                Status = ReferralStatus.Approved
            };

            await BrokerageContext.Referrals.AddAsync(referral);
            await BrokerageContext.SaveChangesAsync();

            return referral;
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
