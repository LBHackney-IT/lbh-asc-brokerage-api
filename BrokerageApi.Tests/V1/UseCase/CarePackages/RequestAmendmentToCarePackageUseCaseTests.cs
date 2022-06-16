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
using NUnit.Framework;

namespace BrokerageApi.Tests.V1.UseCase.CarePackages
{
    public class RequestAmendmentToCarePackageUseCaseTests
    {
        private Mock<ICarePackageGateway> _mockCarePackageGateway;
        private MockDbSaver _mockDbSaver;
        private Mock<IUserService> _mockUserService;
        private Mock<IUserGateway> _mockUserGateway;
        private Mock<IReferralGateway> _mockReferralGateway;
        private RequestAmendmentToCarePackageUseCase _classUnderTest;
        private Fixture _fixture;
        private MockAuditGateway _mockAuditGateway;

        [SetUp]
        public void Setup()
        {
            _fixture = FixtureHelpers.Fixture;
            _mockCarePackageGateway = new Mock<ICarePackageGateway>();
            _mockReferralGateway = new Mock<IReferralGateway>();
            _mockUserService = new Mock<IUserService>();
            _mockUserGateway = new Mock<IUserGateway>();
            _mockDbSaver = new MockDbSaver();
            _mockAuditGateway = new MockAuditGateway();

            _classUnderTest = new RequestAmendmentToCarePackageUseCase(
                _mockCarePackageGateway.Object,
                _mockReferralGateway.Object,
                _mockUserService.Object,
                _mockUserGateway.Object,
                _mockDbSaver.Object,
                _mockAuditGateway.Object
            );
        }

        [Test]
        public async Task CanRequestAmendment()
        {
            const string expectedComment = "comment here";
            var elements = _fixture.BuildElement(1, 1)
                .With(e => e.InternalStatus, ElementStatus.AwaitingApproval)
                .CreateMany();

            var (referral, carePackage) = SetupReferralAndCarePackage(ReferralStatus.AwaitingApproval, 1000, elements.ToArray());

            SetupUser(carePackage.EstimatedYearlyCost + 100);

            await _classUnderTest.ExecuteAsync(referral.Id, expectedComment);

            referral.Status.Should().Be(ReferralStatus.InProgress);

            var amendment = referral.ReferralAmendments.Single();
            amendment.Status.Should().Be(AmendmentStatus.InProgress);
            amendment.Comment.Should().Be(expectedComment);

            _mockDbSaver.VerifyChangesSaved();
        }

        [Test]
        public async Task ThrowsArgumentNullExceptionWhenReferralNotFound()
        {
            const int unknownReferralId = 1234;

            Func<Task> act = () => _classUnderTest.ExecuteAsync(unknownReferralId, "comment");

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

            Func<Task> act = () => _classUnderTest.ExecuteAsync(expectedReferralId, "comment");

            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithMessage($"Care package not found for: {expectedReferralId} (Parameter 'referralId')");
        }

        [Test]
        public async Task ThrowsUnauthorizedWhenUserApprovalLimitTooLow()
        {
            var (referral, carePackage) = SetupReferralAndCarePackage(ReferralStatus.AwaitingApproval, 1000);

            SetupUser(carePackage.EstimatedYearlyCost - 10);

            Func<Task> act = () => _classUnderTest.ExecuteAsync(referral.Id, "comment");

            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("Approver does not have high enough approval limit");
        }

        [Test]
        public async Task ThrowsInvalidOperationWhenCarePackageNotAwaitingApproval([Values] ReferralStatus status)
        {
            var (referral, carePackage) = SetupReferralAndCarePackage(status, 1000);

            SetupUser(carePackage.EstimatedYearlyCost + 10);

            Func<Task> act = () => _classUnderTest.ExecuteAsync(referral.Id, "comment");

            if (status != ReferralStatus.AwaitingApproval)
            {
                await act.Should().ThrowAsync<InvalidOperationException>()
                    .WithMessage("Referral is not in a valid state for approval");
            }
        }

        [Test]
        public async Task AddsAuditEvent()
        {
            const string expectedComment = "comment here";
            var elements = _fixture.BuildElement(1, 1)
                .With(e => e.InternalStatus, ElementStatus.AwaitingApproval)
                .CreateMany();

            var (referral, carePackage) = SetupReferralAndCarePackage(ReferralStatus.AwaitingApproval, 1000, elements.ToArray());

            var expectedUser = SetupUser(carePackage.EstimatedYearlyCost + 100);

            await _classUnderTest.ExecuteAsync(referral.Id, expectedComment);

            _mockAuditGateway.VerifyAuditEventAdded(AuditEventType.AmendmentRequested);
            _mockAuditGateway.LastUserId.Should().Be(expectedUser.Id);
            _mockAuditGateway.LastSocialCareId.Should().Be(referral.SocialCareId);
            var eventMetadata = _mockAuditGateway.LastMetadata.Should().BeOfType<ReferralAuditEventMetadata>().Which;
            eventMetadata.ReferralId.Should().Be(referral.Id);
            eventMetadata.Comment.Should().Be(expectedComment);
        }

        private (Referral referral, CarePackage carePackage) SetupReferralAndCarePackage(ReferralStatus status, decimal estimatedYearlyCost = 0, params Element[] elements)
        {
            var referralBuilder = _fixture.BuildReferral(status)
                .With(r => r.ReferralAmendments, new List<ReferralAmendment>());

            if (elements.Length > 0)
            {
                referralBuilder = referralBuilder.With(r => r.Elements, elements.ToList);
            }

            var referral = referralBuilder.Create();

            var carePackage = _fixture.BuildCarePackage()
                .With(c => c.Id, referral.Id)
                .With(c => c.EstimatedYearlyCost, estimatedYearlyCost)
                .With(c => c.Status, referral.Status)
                .With(c => c.ReferralAmendments, new List<ReferralAmendment>())
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

            _mockUserGateway
                .Setup(x => x.GetByEmailAsync(user.Email))
                .ReturnsAsync(user);

            return user;
        }
    }
}
