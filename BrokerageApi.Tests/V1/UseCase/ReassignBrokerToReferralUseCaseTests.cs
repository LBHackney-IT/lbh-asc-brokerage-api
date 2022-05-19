using System;
using System.Threading.Tasks;
using AutoFixture;
using BrokerageApi.Tests.V1.Helpers;
using BrokerageApi.Tests.V1.UseCase.Mocks;
using BrokerageApi.V1.Boundary.Request;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.Infrastructure.AuditEvents;
using BrokerageApi.V1.Services.Interfaces;
using BrokerageApi.V1.UseCase;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace BrokerageApi.Tests.V1.UseCase
{
    [TestFixture]
    public class ReassignBrokerToReferralUseCaseTests
    {
        private ReassignBrokerToReferralUseCase _classUnderTest;
        private Fixture _fixture;
        private Mock<IReferralGateway> _mockReferralGateway;
        private MockDbSaver _mockDbSaver;
        private MockAuditGateway _mockAuditGateway;
        private Mock<IUserService> _mockUserService;

        [SetUp]
        public void Setup()
        {
            _fixture = FixtureHelpers.Fixture;
            _mockReferralGateway = new Mock<IReferralGateway>();
            _mockAuditGateway = new MockAuditGateway();
            _mockUserService = new Mock<IUserService>();
            _mockDbSaver = new MockDbSaver();
            _classUnderTest = new ReassignBrokerToReferralUseCase(_mockReferralGateway.Object, _mockAuditGateway.Object, _mockUserService.Object, _mockDbSaver.Object);
        }

        [Test]
        public async Task ReassignsBrokerToReferral()
        {
            // Arrange
            var request = _fixture.Build<AssignBrokerRequest>()
                .With(x => x.Broker, "a.broker@hackney.gov.uk")
                .Create();

            var referral = _fixture.Build<Referral>()
                .With(x => x.Status, ReferralStatus.Assigned)
                .With(x => x.AssignedBroker, "other.broker@hackney.gov.uk")
                .Create();

            _mockReferralGateway
                .Setup(x => x.GetByIdAsync(referral.Id))
                .ReturnsAsync(referral);

            _mockDbSaver
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _classUnderTest.ExecuteAsync(referral.Id, request);

            // Assert
            result.Status.Should().Be(ReferralStatus.Assigned);
            result.AssignedBroker.Should().Be("a.broker@hackney.gov.uk");
            _mockDbSaver.VerifyChangesSaved();
        }

        [Test]
        public void ThrowsArgumentNullExceptionWhenReferralDoesntExist()
        {
            // Arrange
            var request = _fixture.Build<AssignBrokerRequest>()
                .With(x => x.Broker, "a.broker@hackney.gov.uk")
                .Create();

            _mockReferralGateway
                .Setup(x => x.GetByIdAsync(123456))
                .ReturnsAsync(null as Referral);

            // Act
            var exception = Assert.ThrowsAsync<ArgumentNullException>(
                async () => await _classUnderTest.ExecuteAsync(123456, request));

            // Assert
            exception.Message.Should().Be("Referral not found for: 123456 (Parameter 'referralId')");
        }

        [Test]
        public async Task ThrowsInvalidOperationExceptionWhenReferralIsUnassigned()
        {
            // Arrange
            var request = _fixture.Build<AssignBrokerRequest>()
                .With(x => x.Broker, "a.broker@hackney.gov.uk")
                .Create();

            var referral = _fixture.Build<Referral>()
                .With(x => x.Status, ReferralStatus.Unassigned)
                .Create();

            _mockReferralGateway
                .Setup(x => x.GetByIdAsync(referral.Id))
                .ReturnsAsync(referral);

            // Act
            Func<Task> act = () => _classUnderTest.ExecuteAsync(referral.Id, request);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Referral is not in a valid state for reassignment");
        }

        [Test]
        public async Task AddsAuditEvent()
        {
            // Arrange
            const int expectedUserId = 1234;
            var request = _fixture.Build<AssignBrokerRequest>()
                .With(x => x.Broker, "a.broker@hackney.gov.uk")
                .Create();

            var referral = _fixture.Build<Referral>()
                .With(x => x.Status, ReferralStatus.Assigned)
                .With(x => x.AssignedBroker, "other.broker@hackney.gov.uk")
                .Create();

            _mockReferralGateway
                .Setup(x => x.GetByIdAsync(referral.Id))
                .ReturnsAsync(referral);

            _mockDbSaver
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            _mockUserService
                .Setup(x => x.UserId)
                .Returns(expectedUserId);

            // Act
            await _classUnderTest.ExecuteAsync(referral.Id, request);

            // Assert
            _mockAuditGateway.VerifyAuditEventAdded(AuditEventType.ReferralBrokerReassignment);
            _mockAuditGateway.LastUserId.Should().Be(expectedUserId);
            _mockAuditGateway.LastSocialCareId.Should().Be(referral.SocialCareId);
            var eventMetadata = _mockAuditGateway.LastMetadata.Should().BeOfType<ReferralReassignmentAuditEventMetadata>().Which;
            eventMetadata.AssignedBrokerName.Should().Be(request.Broker);
        }
    }
}
