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

namespace BrokerageApi.Tests.V1.UseCase.CarePackages
{
    [TestFixture]
    public class AssignBudgetApproverToCarePackageUseCaseTests
    {
        private AssignBudgetApproverToCarePackageUseCase _classUnderTest;
        private Fixture _fixture;
        private Mock<ICarePackageGateway> _mockCarePackageGateway;
        private MockDbSaver _mockDbSaver;
        private MockAuditGateway _mockAuditGateway;
        private Mock<IUserGateway> _mockUserGateway;
        private Mock<IUserService> _mockUserService;
        private Mock<IReferralGateway> _mockReferralGateway;

        [SetUp]
        public void Setup()
        {
            _fixture = FixtureHelpers.Fixture;

            _mockCarePackageGateway = new Mock<ICarePackageGateway>();
            _mockReferralGateway = new Mock<IReferralGateway>();
            _mockAuditGateway = new MockAuditGateway();
            _mockDbSaver = new MockDbSaver();
            _mockUserGateway = new Mock<IUserGateway>();
            _mockUserService = new Mock<IUserService>();

            _classUnderTest = new AssignBudgetApproverToCarePackageUseCase(
                _mockCarePackageGateway.Object,
                _mockReferralGateway.Object,
                _mockUserGateway.Object,
                _mockUserService.Object,
                _mockAuditGateway.Object,
                _mockDbSaver.Object
            );
        }

        [Test]
        public async Task CanAssignBudgetApprover()
        {
            const int expectedUserId = 1234;
            const string brokerEmail = "broker@email.com";
            const string approverEmail = "approver@email.com";
            const decimal estimatedYearlyCost = 1000;

            var referral = _fixture.BuildReferral(ReferralStatus.InProgress)
                .With(r => r.AssignedBroker, brokerEmail)
                .Create();

            var carePackage = _fixture.BuildCarePackage()
                .With(c => c.Id, referral.Id)
                .With(c => c.Status, ReferralStatus.InProgress)
                .With(c => c.AssignedBrokerId, brokerEmail)
                .With(c => c.WeeklyPayment, estimatedYearlyCost / 52)
                .Create();

            var approver = _fixture.BuildUser()
                .With(u => u.ApprovalLimit, estimatedYearlyCost + 100)
                .Create();

            _mockReferralGateway
                .Setup(x => x.GetByIdAsync(referral.Id))
                .ReturnsAsync(referral);

            _mockCarePackageGateway
                .Setup(x => x.GetByIdAsync(referral.Id))
                .ReturnsAsync(carePackage);

            _mockUserGateway
                .Setup(x => x.GetByEmailAsync(approverEmail))
                .ReturnsAsync(approver);

            _mockUserService
                .Setup(x => x.Email)
                .Returns(brokerEmail);

            _mockUserService
                .Setup(x => x.UserId)
                .Returns(expectedUserId);

            await _classUnderTest.ExecuteAsync(referral.Id, approverEmail);

            referral.Status.Should().Be(ReferralStatus.AwaitingApproval);
            referral.AssignedBroker.Should().BeEquivalentTo(approver.Email);
            _mockDbSaver.VerifyChangesSaved();
        }

        [Test]
        public async Task ThrowsArgumentNullExceptionWhenCarePackageNotFound()
        {
            const string approverEmail = "approver@email.com";

            const int unknownReferralId = 1234;

            _mockReferralGateway
                .Setup(x => x.GetByIdAsync(unknownReferralId))
                .ReturnsAsync((Referral) null);

            Func<Task> act = () => _classUnderTest.ExecuteAsync(unknownReferralId, approverEmail);

            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithMessage($"Care package not found for: {unknownReferralId} (Parameter 'referralId')");
            _mockDbSaver.VerifyChangesNotSaved();
        }

        [Test]
        public async Task ThrowsUnauthorizedAccessExceptionWhenCarePackageNotAssignedToUser()
        {
            const string brokerEmail = "broker@email.com";
            const string assignedBrokerEmail = "another.broker@email.com";
            const string approverEmail = "approver@email.com";

            var referral = _fixture.BuildReferral(ReferralStatus.InProgress)
                .With(r => r.AssignedBroker, assignedBrokerEmail)
                .Create();

            _mockReferralGateway
                .Setup(x => x.GetByIdAsync(referral.Id))
                .ReturnsAsync(referral);

            _mockUserService
                .Setup(x => x.Email)
                .Returns(brokerEmail);

            Func<Task> act = () => _classUnderTest.ExecuteAsync(referral.Id, approverEmail);

            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage($"Referral is not assigned to {brokerEmail}");
            _mockDbSaver.VerifyChangesNotSaved();
        }

        [Test]
        public async Task ThrowsInvalidOperationExceptionWhenCarePackageNotInProgress([Values] ReferralStatus status)
        {
            const string brokerEmail = "broker@email.com";
            const string approverEmail = "approver@email.com";

            var referral = _fixture.BuildReferral(status)
                .With(r => r.AssignedBroker, brokerEmail)
                .Create();

            _mockReferralGateway
                .Setup(x => x.GetByIdAsync(referral.Id))
                .ReturnsAsync(referral);

            _mockUserService
                .Setup(x => x.Email)
                .Returns(brokerEmail);

            Func<Task> act = () => _classUnderTest.ExecuteAsync(referral.Id, approverEmail);

            if (status != ReferralStatus.InProgress)
            {
                await act.Should().ThrowAsync<InvalidOperationException>()
                    .WithMessage("Referral is not in a valid state to start editing");
                _mockDbSaver.VerifyChangesNotSaved();
            }
        }

        [Test]
        public async Task ThrowsArgumentNullExceptionWhenApproverNotFound()
        {
            const string brokerEmail = "broker@email.com";
            const string approverEmail = "approver@email.com";

            var referral = _fixture.BuildReferral(ReferralStatus.InProgress)
                .With(r => r.AssignedBroker, brokerEmail)
                .Create();

            _mockReferralGateway
                .Setup(x => x.GetByIdAsync(referral.Id))
                .ReturnsAsync(referral);

            _mockUserGateway
                .Setup(x => x.GetByEmailAsync(approverEmail))
                .ReturnsAsync((User) null);

            _mockUserService
                .Setup(x => x.Email)
                .Returns(brokerEmail);

            Func<Task> act = () => _classUnderTest.ExecuteAsync(referral.Id, approverEmail);

            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithMessage($"Approver not found with: {approverEmail} (Parameter 'budgetApproverEmail')");
            _mockDbSaver.VerifyChangesNotSaved();
        }

        [Test]
        public async Task ThrowsUnauthorizedAccessExceptionWhenApproverLimitBelowYearlyEstimate()
        {
            const string brokerEmail = "broker@email.com";
            const string approverEmail = "approver@email.com";
            const decimal estimatedYearlyCost = 1000;

            var referral = _fixture.BuildReferral(ReferralStatus.InProgress)
                .With(r => r.AssignedBroker, brokerEmail)
                .Create();

            var carePackage = _fixture.BuildCarePackage()
                .With(c => c.Id, referral.Id)
                .With(c => c.Status, ReferralStatus.InProgress)
                .With(c => c.AssignedBrokerId, brokerEmail)
                .With(c => c.WeeklyCost, estimatedYearlyCost / 52)
                .Create();

            var approver = _fixture.BuildUser()
                .With(u => u.ApprovalLimit, estimatedYearlyCost - 100)
                .Create();

            _mockReferralGateway
                .Setup(x => x.GetByIdAsync(referral.Id))
                .ReturnsAsync(referral);

            _mockCarePackageGateway
                .Setup(x => x.GetByIdAsync(referral.Id))
                .ReturnsAsync(carePackage);

            _mockUserGateway
                .Setup(x => x.GetByEmailAsync(approverEmail))
                .ReturnsAsync(approver);

            _mockUserService
                .Setup(x => x.Email)
                .Returns(brokerEmail);

            Func<Task> act = () => _classUnderTest.ExecuteAsync(referral.Id, approverEmail);

            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("Approver does not have high enough approval limit");
            _mockDbSaver.VerifyChangesNotSaved();
        }

        [Test]
        public async Task AddsAuditTrail()
        {
            const int expectedUserId = 1234;
            const string brokerEmail = "broker@email.com";
            const string approverEmail = "approver@email.com";
            const decimal estimatedYearlyCost = 1000;

            var referral = _fixture.BuildReferral(ReferralStatus.InProgress)
                .With(r => r.AssignedBroker, brokerEmail)
                .Create();

            var carePackage = _fixture.BuildCarePackage()
                .With(c => c.Id, referral.Id)
                .With(c => c.Status, ReferralStatus.InProgress)
                .With(c => c.AssignedBrokerId, brokerEmail)
                .With(c => c.WeeklyPayment, estimatedYearlyCost / 52)
                .Create();

            var approver = _fixture.BuildUser()
                .With(u => u.ApprovalLimit, estimatedYearlyCost + 100)
                .Create();

            _mockReferralGateway
                .Setup(x => x.GetByIdAsync(referral.Id))
                .ReturnsAsync(referral);

            _mockCarePackageGateway
                .Setup(x => x.GetByIdAsync(referral.Id))
                .ReturnsAsync(carePackage);

            _mockUserGateway
                .Setup(x => x.GetByEmailAsync(approverEmail))
                .ReturnsAsync(approver);

            _mockUserService
                .Setup(x => x.Email)
                .Returns(brokerEmail);

            _mockUserService
                .Setup(x => x.UserId)
                .Returns(expectedUserId);

            await _classUnderTest.ExecuteAsync(referral.Id, approverEmail);

            _mockAuditGateway.VerifyAuditEventAdded(AuditEventType.CarePackageBudgetApproverAssigned);
            _mockAuditGateway.LastUserId.Should().Be(expectedUserId);
            _mockAuditGateway.LastSocialCareId.Should().Be(referral.SocialCareId);
            var eventMetadata = _mockAuditGateway.LastMetadata.Should().BeOfType<BudgetApproverAssignmentAuditEventMetadata>().Which;
            eventMetadata.ReferralId.Should().Be(referral.Id);
            eventMetadata.AssignedApproverName.Should().Be(approver.Name);
        }
    }
}
