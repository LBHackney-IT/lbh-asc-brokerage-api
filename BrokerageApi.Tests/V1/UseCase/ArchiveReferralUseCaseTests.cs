using System;
using System.Threading.Tasks;
using AutoFixture;
using BrokerageApi.Tests.V1.Helpers;
using BrokerageApi.Tests.V1.UseCase.Mocks;
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
    public class ArchiveReferralUseCaseTests
    {
        private Mock<IReferralGateway> _mockReferralGateway;
        private ArchiveReferralUseCase _classUnderTest;
        private Fixture _fixture;
        private MockDbSaver _mockDbSaver;
        private MockAuditGateway _mockAuditGateway;
        private Mock<IUserService> _mockUserService;

        [SetUp]
        public void Setup()
        {
            _fixture = FixtureHelpers.Fixture;
            _mockReferralGateway = new Mock<IReferralGateway>();
            _mockDbSaver = new MockDbSaver();
            _mockAuditGateway = new MockAuditGateway();
            _mockUserService = new Mock<IUserService>();

            _classUnderTest = new ArchiveReferralUseCase(
                _mockReferralGateway.Object,
                _mockDbSaver.Object,
                _mockAuditGateway.Object,
                _mockUserService.Object
                );
        }

        [Test]
        public async Task CanArchiveReferral()
        {
            const string expectedComment = "commentHere";
            var referral = _fixture.BuildReferral(ReferralStatus.InProgress)
                .Create();
            _mockReferralGateway.Setup(x => x.GetByIdAsync(referral.Id))
                .ReturnsAsync(referral);

            await _classUnderTest.ExecuteAsync(referral.Id, expectedComment);

            referral.Status.Should().Be(ReferralStatus.Archived);
            referral.Comment.Should().Be(expectedComment);
            _mockDbSaver.VerifyChangesSaved();
        }

        [Test]
        public async Task AddsAuditTrail()
        {
            const string expectedComment = "commentHere";
            const int expectedUserId = 1234;
            var referral = _fixture.BuildReferral(ReferralStatus.InProgress)
                .Create();
            _mockReferralGateway.Setup(x => x.GetByIdAsync(referral.Id))
                .ReturnsAsync(referral);
            _mockUserService.Setup(x => x.UserId)
                .Returns(expectedUserId);

            await _classUnderTest.ExecuteAsync(referral.Id, expectedComment);

            _mockAuditGateway.VerifyAuditEventAdded(AuditEventType.ReferralArchived);
            _mockAuditGateway.LastUserId.Should().Be(expectedUserId);
            _mockAuditGateway.LastSocialCareId.Should().Be(referral.SocialCareId);
            var eventMetadata = _mockAuditGateway.LastMetadata.Should().BeOfType<ReferralAuditEventMetadata>().Which;
            eventMetadata.ReferralId.Should().Be(referral.Id);
            eventMetadata.Comment.Should().Be(expectedComment);
        }

        [Test]
        public async Task ThrowsArgumentNullExceptionWhenReferralDoesntExist()
        {
            const int unknownReferralId = 1234;
            _mockReferralGateway
                .Setup(x => x.GetByIdAsync(unknownReferralId))
                .ReturnsAsync(null as Referral);

            Func<Task> act = () => _classUnderTest.ExecuteAsync(unknownReferralId, "");

            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithMessage($"Referral not found for: {unknownReferralId} (Parameter 'referralId')");
            _mockDbSaver.VerifyChangesNotSaved();
        }

        [Test]
        public async Task ThrowsInvalidOperationExceptionWhenReferralNotInProgress([Values] ReferralStatus status)
        {
            var referral = _fixture.BuildReferral(status)
                .Create();
            _mockReferralGateway.Setup(x => x.GetByIdAsync(referral.Id))
                .ReturnsAsync(referral);

            Func<Task> act = () => _classUnderTest.ExecuteAsync(referral.Id, "");

            if (status != ReferralStatus.InProgress)
            {
                await act.Should().ThrowAsync<InvalidOperationException>()
                    .WithMessage("Referral is not in a valid state for archive");
                _mockDbSaver.VerifyChangesNotSaved();
            }
        }
    }
}
