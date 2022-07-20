using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using BrokerageApi.Tests.V1.Helpers;
using BrokerageApi.Tests.V1.UseCase.Mocks;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.Infrastructure.AuditEvents;
using BrokerageApi.V1.Services.Interfaces;
using BrokerageApi.V1.UseCase.CarePackages;
using FluentAssertions;
using Moq;
using NodaTime;
using NUnit.Framework;

namespace BrokerageApi.Tests.V1.UseCase.CarePackages
{
    public class RequestFollowUpToCarePackageUseCaseTests
    {
        private Mock<ICarePackageGateway> _mockCarePackageGateway;
        private Mock<IReferralGateway> _mockReferralGateway;
        private Mock<IUserService> _mockUserService;
        private MockDbSaver _mockDbSaver;
        private MockAuditGateway _mockAuditGateway;
        private Mock<IClockService> _mockClock;
        private RequestFollowUpToCarePackageUseCase _classUnderTest;
        private Fixture _fixture;
        private IClockService _clock;

        [SetUp]
        public void Setup()
        {
            _fixture = FixtureHelpers.Fixture;
            _mockCarePackageGateway = new Mock<ICarePackageGateway>();
            _mockReferralGateway = new Mock<IReferralGateway>();
            _mockUserService = new Mock<IUserService>();
            _mockDbSaver = new MockDbSaver();
            _mockAuditGateway = new MockAuditGateway();
            _mockClock = new Mock<IClockService>();

            _clock = _mockClock.Object;

            _classUnderTest = new RequestFollowUpToCarePackageUseCase(
                _mockCarePackageGateway.Object,
                _mockReferralGateway.Object,
                _mockUserService.Object,
                _mockDbSaver.Object,
                _mockAuditGateway.Object,
                _mockClock.Object
            );
        }

        [Test]
        public async Task CanRequestFollowUp()
        {
            const string expectedComment = "comment here";
            var expectedDate = _clock.Today.PlusDays(30);

            var elements = _fixture.BuildElement(1, 1)
                .With(e => e.InternalStatus, ElementStatus.Approved)
                .CreateMany();

            var (referral, carePackage) = SetupReferralAndCarePackage(ReferralStatus.Approved, 1000, elements.ToArray());

            var user = SetupUser(carePackage.EstimatedYearlyCost + 100);

            await _classUnderTest.ExecuteAsync(referral.Id, expectedComment, expectedDate);

            referral.Status.Should().Be(ReferralStatus.Approved);

            var followUp = referral.ReferralFollowUps.Single();
            followUp.Status.Should().Be(FollowUpStatus.InProgress);
            followUp.Comment.Should().Be(expectedComment);
            followUp.Date.Should().Be(expectedDate);
            followUp.RequestedAt.Should().Be(_clock.Now);
            followUp.RequestedByEmail.Should().Be(user.Email);

            _mockDbSaver.VerifyChangesSaved();
        }

        [Test]
        public async Task ThrowsArgumentNullExceptionWhenReferralNotFound()
        {
            const int unknownReferralId = 1234;

            Func<Task> act = () => _classUnderTest.ExecuteAsync(unknownReferralId, "comment", _clock.Today);

            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithMessage($"Referral not found for: {unknownReferralId} (Parameter 'referralId')");
        }

        [Test]
        public async Task ThrowsArgumentNullExceptionWhenCarePackageNotFound()
        {
            const int expectedReferralId = 1234;
            _mockReferralGateway
                .Setup(x => x.GetByIdAsync(expectedReferralId))
                .ReturnsAsync(new Referral());

            Func<Task> act = () => _classUnderTest.ExecuteAsync(expectedReferralId, "comment", _clock.Today);

            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithMessage($"Care package not found for: {expectedReferralId} (Parameter 'referralId')");
        }

        [Test]
        public async Task ThrowsInvalidOperationWhenCarePackageNotAwaitingApproval([Values] ReferralStatus status)
        {
            var (referral, carePackage) = SetupReferralAndCarePackage(status, 1000);

            SetupUser(carePackage.EstimatedYearlyCost + 10);

            Func<Task> act = () => _classUnderTest.ExecuteAsync(referral.Id, "comment", _clock.Today);

            if (status != ReferralStatus.Approved)
            {
                await act.Should().ThrowAsync<InvalidOperationException>()
                    .WithMessage("Referral is not in a valid state for requesting a follow-up");
            }
        }

        [Test]
        public async Task AddsAuditEvent()
        {
            const string expectedComment = "comment here";
            var expectedDate = _clock.Today.PlusDays(30);

            var elements = _fixture.BuildElement(1, 1)
                .With(e => e.InternalStatus, ElementStatus.Approved)
                .CreateMany();

            var (referral, carePackage) = SetupReferralAndCarePackage(ReferralStatus.Approved, 1000, elements.ToArray());

            var expectedUser = SetupUser(carePackage.EstimatedYearlyCost + 100);

            await _classUnderTest.ExecuteAsync(referral.Id, expectedComment, expectedDate);

            _mockAuditGateway.VerifyAuditEventAdded(AuditEventType.FollowUpRequested);
            _mockAuditGateway.LastUserId.Should().Be(expectedUser.Id);
            _mockAuditGateway.LastSocialCareId.Should().Be(referral.SocialCareId);
            var eventMetadata = _mockAuditGateway.LastMetadata.Should().BeOfType<ReferralFollowUpAuditEventMetadata>().Which;
            eventMetadata.ReferralId.Should().Be(referral.Id);
            eventMetadata.Comment.Should().Be(expectedComment);
            eventMetadata.Date.Should().Be(expectedDate);
        }

        private (Referral referral, CarePackage carePackage) SetupReferralAndCarePackage(ReferralStatus status, decimal estimatedYearlyCost = 0, params Element[] elements)
        {
            var referralBuilder = _fixture.BuildReferral(status)
                .With(r => r.ReferralFollowUps, new List<ReferralFollowUp>());

            if (elements.Length > 0)
            {
                referralBuilder = referralBuilder.With(r => r.Elements, elements.ToList);
            }

            var referral = referralBuilder.Create();

            var carePackage = _fixture.BuildCarePackage()
                .With(c => c.Id, referral.Id)
                .With(c => c.EstimatedYearlyCost, estimatedYearlyCost)
                .With(c => c.Status, referral.Status)
                .With(c => c.ReferralFollowUps, new List<ReferralFollowUp>())
                .Create();

            _mockReferralGateway
                .Setup(x => x.GetByIdAsync(referral.Id))
                .ReturnsAsync(referral);

            _mockCarePackageGateway
                .Setup(x => x.GetByIdAsync(referral.Id))
                .ReturnsAsync(carePackage);

            return (referral, carePackage);
        }

        private User SetupUser(decimal approvalLimit)
        {
            var user = _fixture.BuildUser()
                .With(u => u.ApprovalLimit, approvalLimit)
                .Create();

            _mockUserService
                .Setup(x => x.UserId)
                .Returns(user.Id);

            _mockUserService
                .Setup(x => x.Email)
                .Returns(user.Email);

            return user;
        }
    }
}
