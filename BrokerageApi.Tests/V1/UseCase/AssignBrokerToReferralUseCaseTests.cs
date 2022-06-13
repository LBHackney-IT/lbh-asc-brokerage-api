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
    public class AssignBrokerToReferralUseCaseTests
    {
        private AssignBrokerToReferralUseCase _classUnderTest;
        private Fixture _fixture;
        private Mock<IReferralGateway> _mockReferralGateway;
        private MockDbSaver _mockDbSaver;
        private MockAuditGateway _mockAuditGateway;
        private Mock<IUserService> _mockUserService;
        private Mock<IUserGateway> _mockUserGateway;

        [SetUp]
        public void Setup()
        {
            _fixture = FixtureHelpers.Fixture;
            _mockReferralGateway = new Mock<IReferralGateway>();
            _mockAuditGateway = new MockAuditGateway();
            _mockUserService = new Mock<IUserService>();
            _mockDbSaver = new MockDbSaver();
            _mockUserGateway = new Mock<IUserGateway>();
            _classUnderTest = new AssignBrokerToReferralUseCase(
                _mockReferralGateway.Object,
                _mockAuditGateway.Object,
                _mockUserService.Object,
                _mockDbSaver.Object,
                _mockUserGateway.Object
            );
        }


        [Test]
        public async Task AssignsBrokerToReferral()
        {
            // Arrange
            var expectedBroker = _fixture.BuildUser().Create();

            var request = _fixture.Build<AssignBrokerRequest>()
                .With(x => x.Broker, expectedBroker.Email)
                .Create();

            var referral = _fixture.BuildReferral(ReferralStatus.Unassigned)
                .Create();

            _mockReferralGateway
                .Setup(x => x.GetByIdAsync(referral.Id))
                .ReturnsAsync(referral);

            _mockDbSaver
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            _mockUserGateway
                .Setup(x => x.GetByEmailAsync(expectedBroker.Email))
                .ReturnsAsync(expectedBroker);

            // Act
            var result = await _classUnderTest.ExecuteAsync(referral.Id, request);

            // Assert
            result.Status.Should().Be(ReferralStatus.Assigned);
            result.AssignedBrokerEmail.Should().Be(expectedBroker.Email);
            _mockDbSaver.VerifyChangesSaved();
        }

        [Test]
        public async Task ThrowsArgumentNullExceptionWhenReferralDoesntExist()
        {
            // Arrange
            const int unknownReferral = 1234;
            var request = _fixture.Build<AssignBrokerRequest>()
                .With(x => x.Broker, "a.broker@hackney.gov.uk")
                .Create();

            _mockReferralGateway
                .Setup(x => x.GetByIdAsync(unknownReferral))
                .ReturnsAsync(null as Referral);

            // Act
            Func<Task> act = () => _classUnderTest.ExecuteAsync(unknownReferral, request);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithMessage($"Referral not found for: {unknownReferral} (Parameter 'referralId')");
            _mockDbSaver.VerifyChangesNotSaved();
        }

        [Test]
        public async Task ThrowsArgumentNullExceptionWhenBrokerNotFound()
        {
            // Arrange
            const string expectedBroker = "a.broker@hackney.gov.uk";

            var request = _fixture.Build<AssignBrokerRequest>()
                .With(x => x.Broker, expectedBroker)
                .Create();

            var referral = _fixture.BuildReferral(ReferralStatus.Unassigned)
                .Create();

            _mockReferralGateway
                .Setup(x => x.GetByIdAsync(referral.Id))
                .ReturnsAsync(referral);

            _mockDbSaver
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            Func<Task> act = () => _classUnderTest.ExecuteAsync(referral.Id, request);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithMessage($"Broker not found for: {request.Broker} (Parameter 'request')");
            _mockDbSaver.VerifyChangesNotSaved();
        }

        [Test]
        public async Task ThrowsInvalidOperationExceptionWhenReferralIsNotUnassigned()
        {
            // Arrange
            var request = _fixture.Build<AssignBrokerRequest>()
                .With(x => x.Broker, "a.broker@hackney.gov.uk")
                .Create();

            var referral = _fixture.BuildReferral(ReferralStatus.Assigned)
                .Create();

            _mockReferralGateway
                .Setup(x => x.GetByIdAsync(referral.Id))
                .ReturnsAsync(referral);

            // Act
            Func<Task> act = () => _classUnderTest.ExecuteAsync(referral.Id, request);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Referral is not in a valid state for assignment");
        }

        [Test]
        public async Task AddsAuditEvent()
        {
            // Arrange
            const int expectedUserId = 1234;
            var expectedBroker = _fixture.BuildUser().Create();
            var request = _fixture.Build<AssignBrokerRequest>()
                .With(x => x.Broker, "a.broker@hackney.gov.uk")
                .Create();

            var referral = _fixture.BuildReferral(ReferralStatus.Unassigned)
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

            _mockUserGateway
                .Setup(x => x.GetByEmailAsync(request.Broker))
                .ReturnsAsync(expectedBroker);

            // Act
            await _classUnderTest.ExecuteAsync(referral.Id, request);

            // Assert
            _mockAuditGateway.VerifyAuditEventAdded(AuditEventType.ReferralBrokerAssignment);
            _mockAuditGateway.LastUserId.Should().Be(expectedUserId);
            _mockAuditGateway.LastSocialCareId.Should().Be(referral.SocialCareId);
            var eventMetadata = _mockAuditGateway.LastMetadata.Should().BeOfType<ReferralAssignmentAuditEventMetadata>().Which;
            eventMetadata.AssignedBrokerName.Should().Be(expectedBroker.Name);
            eventMetadata.ReferralId.Should().Be(referral.Id);
        }
    }

}
