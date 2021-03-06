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
using JetBrains.Annotations;
using Moq;
using NodaTime;
using NUnit.Framework;

namespace BrokerageApi.Tests.V1.UseCase.CarePackages
{

    public class ApproveCarePackageUseCaseTests
    {
        private Mock<ICarePackageGateway> _mockCarePackageGateway;
        private MockDbSaver _mockDbSaver;
        private Mock<IUserService> _mockUserService;
        private Mock<IUserGateway> _mockUserGateway;
        private Mock<IReferralGateway> _mockReferralGateway;
        private ApproveCarePackageUseCase _classUnderTest;
        private Fixture _fixture;
        private MockAuditGateway _mockAuditGateway;
        private Mock<IClockService> _mockClock;
        private IClockService _clock;

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
            _mockClock = new Mock<IClockService>();

            _clock = _mockClock.Object;

            _classUnderTest = new ApproveCarePackageUseCase(
                _mockCarePackageGateway.Object,
                _mockReferralGateway.Object,
                _mockUserService.Object,
                _mockUserGateway.Object,
                _mockDbSaver.Object,
                _mockAuditGateway.Object,
                _mockClock.Object
            );
        }

        [Test]
        public async Task UpdatesStatusToApproved()
        {
            var elements = _fixture.BuildElement(1, 1)
                .With(e => e.InternalStatus, ElementStatus.AwaitingApproval)
                .CreateMany();

            var (referral, carePackage) = SetupReferralAndCarePackage(ReferralStatus.AwaitingApproval, 1000, elements.ToArray());

            SetupUser(carePackage.EstimatedYearlyCost + 100);

            await _classUnderTest.ExecuteAsync(referral.Id);

            referral.Status.Should().Be(ReferralStatus.Approved);
            referral.Elements.Should().OnlyContain(e => e.InternalStatus == ElementStatus.Approved);

            _mockDbSaver.VerifyChangesSaved();
        }

        [Test]
        public async Task EndsAllParentElements()
        {
            var startDate = LocalDate.FromDateTime(DateTime.Today);

            var parentElement = _fixture.BuildElement(1, 1)
                .With(e => e.InternalStatus, ElementStatus.Approved)
                .With(e => e.EndDate, startDate.PlusDays(100))
                .Create();

            var element = _fixture.BuildElement(1, 1)
                .With(e => e.InternalStatus, ElementStatus.AwaitingApproval)
                .With(e => e.StartDate, startDate)
                .With(e => e.ParentElement, parentElement)
                .Create();

            var (referral, carePackage) = SetupReferralAndCarePackage(ReferralStatus.AwaitingApproval, 1000, element, parentElement);

            SetupUser(carePackage.EstimatedYearlyCost + 100);

            await _classUnderTest.ExecuteAsync(referral.Id);

            parentElement.EndDate.Should().Be(startDate.PlusDays(-1));

            _mockDbSaver.VerifyChangesSaved();
        }

        [Test]
        public async Task UpdatesStatusOfOldReferral()
        {

            var (referral, carePackage) = SetupReferralAndCarePackage(ReferralStatus.AwaitingApproval, 1000);

            var oldApprovedReferral = _fixture.BuildReferral(ReferralStatus.Approved).Create();
            var oldEndedReferral = _fixture.BuildReferral(ReferralStatus.Ended).Create();
            var oldArchivedReferral = _fixture.BuildReferral(ReferralStatus.Archived).Create();
            _mockReferralGateway
                .Setup(x => x.GetBySocialCareIdWithElementsAsync(referral.SocialCareId))
                .ReturnsAsync(new List<Referral> { oldApprovedReferral, oldEndedReferral, oldArchivedReferral });

            SetupUser(carePackage.EstimatedYearlyCost + 100);

            await _classUnderTest.ExecuteAsync(referral.Id);

            oldApprovedReferral.Status.Should().Be(ReferralStatus.Ended);
            oldEndedReferral.Status.Should().Be(ReferralStatus.Ended);
            oldArchivedReferral.Status.Should().Be(ReferralStatus.Archived);

            _mockDbSaver.VerifyChangesSaved();
        }

        [Test]
        public async Task UpdatesIsResidential()
        {
            // Arrange
            var service = _fixture.BuildService()
                .Create();

            var elementType = _fixture.BuildElementType(service.Id)
                .With(et => et.Service, service)
                .With(et => et.IsResidential, true)
                .Create();

            var element = _fixture.BuildElement(elementType.Id)
                .With(e => e.ElementType, elementType)
                .Create();

            var elements = new Element[] { element };

            var (referral, carePackage) = SetupReferralAndCarePackage(ReferralStatus.AwaitingApproval, 1000, elements);

            SetupUser(carePackage.EstimatedYearlyCost + 10);

            // Act
            await _classUnderTest.ExecuteAsync(referral.Id);

            // Assert
            referral.Status.Should().Be(ReferralStatus.Approved);
            referral.IsResidential.Should().BeTrue();

            _mockDbSaver.VerifyChangesSaved();
        }

        [Test]
        public async Task UpdatesCareChargeStatusToExistingIfOldReferralLessThanSixMonths()
        {
            // Arrange
            var (referral, carePackage) = SetupReferralAndCarePackage(ReferralStatus.AwaitingApproval, 1000);

            var threeMonthsAgo = _clock.Now.Minus(Duration.FromDays(90));

            var oldReferral = _fixture.BuildReferral(ReferralStatus.Approved)
                .With(r => r.CareChargesConfirmedAt, threeMonthsAgo)
                .Create();

            _mockReferralGateway
                .Setup(x => x.GetBySocialCareIdWithElementsAsync(referral.SocialCareId))
                .ReturnsAsync(new List<Referral> { oldReferral });

            SetupUser(carePackage.EstimatedYearlyCost + 10);

            // Act
            await _classUnderTest.ExecuteAsync(referral.Id);

            // Assert
            referral.Status.Should().Be(ReferralStatus.Approved);
            referral.CareChargeStatus.Should().Be(CareChargeStatus.Existing);

            _mockDbSaver.VerifyChangesSaved();
        }

        [Test]
        public async Task UpdatesCareChargeStatusToCancellationIfAllElementsCancelled()
        {
            // Arrange
            var service = _fixture.BuildService().Create();
            var provider = _fixture.BuildProvider().Create();
            var elementType = _fixture.BuildElementType(service.Id).Create();

            var element = _fixture.BuildElement(elementType.Id, provider.Id)
                .With(e => e.ElementType, elementType)
                .With(e => e.InternalStatus, ElementStatus.AwaitingApproval)
                .Create();

            var referral = _fixture.BuildReferral(ReferralStatus.AwaitingApproval)
                .With(r => r.Elements, new List<Element> { element })
                .Create();

            var referralElement = _fixture.BuildReferralElement(referral.Id, element.Id)
                .With(re => re.PendingCancellation, true)
                .Create();

            element.ReferralElements = new List<ReferralElement> { referralElement };
            referral.ReferralElements = new List<ReferralElement> { referralElement };

            _mockReferralGateway
                .Setup(x => x.GetByIdWithElementsAsync(referral.Id))
                .ReturnsAsync(referral);

            _mockCarePackageGateway
                .Setup(x => x.GetByIdAsync(referral.Id))
                .ReturnsAsync(new CarePackage { Id = referral.Id, Status = referral.Status });

            var expectedUser = SetupUser(element.Cost * 100);

            // Act
            await _classUnderTest.ExecuteAsync(referral.Id);

            // Assert
            referral.Status.Should().Be(ReferralStatus.Approved);
            referral.ServiceElements.Count.Should().Be(1);
            referral.CareChargeStatus.Should().Be(CareChargeStatus.Cancellation);

            _mockDbSaver.VerifyChangesSaved();
        }

        [Test]
        public async Task UpdatesCareChargeStatusToTerminationIfAllElementsEnded()
        {
            // Arrange
            var endDate = LocalDate.FromDateTime(DateTime.Today).PlusDays(-1);
            var service = _fixture.BuildService().Create();
            var provider = _fixture.BuildProvider().Create();
            var elementType = _fixture.BuildElementType(service.Id).Create();

            var element = _fixture.BuildElement(elementType.Id, provider.Id)
                .With(e => e.ElementType, elementType)
                .With(e => e.InternalStatus, ElementStatus.AwaitingApproval)
                .Create();

            var referral = _fixture.BuildReferral(ReferralStatus.AwaitingApproval)
                .With(r => r.Elements, new List<Element> { element })
                .Create();

            var referralElement = _fixture.BuildReferralElement(referral.Id, element.Id)
                .With(re => re.PendingEndDate, endDate)
                .Create();

            element.ReferralElements = new List<ReferralElement> { referralElement };
            referral.ReferralElements = new List<ReferralElement> { referralElement };

            _mockReferralGateway
                .Setup(x => x.GetByIdWithElementsAsync(referral.Id))
                .ReturnsAsync(referral);

            _mockCarePackageGateway
                .Setup(x => x.GetByIdAsync(referral.Id))
                .ReturnsAsync(new CarePackage { Id = referral.Id, Status = referral.Status });

            var expectedUser = SetupUser(element.Cost * 100);

            // Act
            await _classUnderTest.ExecuteAsync(referral.Id);

            // Assert
            referral.Status.Should().Be(ReferralStatus.Approved);
            referral.ServiceElements.Count.Should().Be(1);
            referral.CareChargeStatus.Should().Be(CareChargeStatus.Termination);

            _mockDbSaver.VerifyChangesSaved();
        }

        [Test]
        public async Task UpdatesCareChargeStatusToSuspensionIfAllElementsSuspended()
        {
            // Arrange
            var startDate = LocalDate.FromDateTime(DateTime.Today).PlusDays(-180);
            var suspensionDate = LocalDate.FromDateTime(DateTime.Today).PlusDays(-3);
            var service = _fixture.BuildService().Create();
            var provider = _fixture.BuildProvider().Create();
            var elementType = _fixture.BuildElementType(service.Id).Create();

            var element = _fixture.BuildElement(elementType.Id, provider.Id)
                .With(e => e.ElementType, elementType)
                .With(e => e.InternalStatus, ElementStatus.AwaitingApproval)
                .With(e => e.StartDate, startDate)
                .Create();

            var suspensionElement = _fixture.BuildElement(elementType.Id, provider.Id)
                .With(e => e.ElementType, elementType)
                .With(e => e.InternalStatus, ElementStatus.AwaitingApproval)
                .With(e => e.StartDate, suspensionDate)
                .With(e => e.IsSuspension, true)
                .With(e => e.SuspendedElementId, element.Id)
                .With(e => e.SuspendedElement, element)
                .Create();

            element.SuspensionElements = new List<Element> { suspensionElement };

            var referral = _fixture.BuildReferral(ReferralStatus.AwaitingApproval)
                .With(r => r.Elements, new List<Element> { element, suspensionElement })
                .Create();

            _mockReferralGateway
                .Setup(x => x.GetByIdWithElementsAsync(referral.Id))
                .ReturnsAsync(referral);

            _mockCarePackageGateway
                .Setup(x => x.GetByIdAsync(referral.Id))
                .ReturnsAsync(new CarePackage { Id = referral.Id, Status = referral.Status });

            var expectedUser = SetupUser(element.Cost * 100);

            // Act
            await _classUnderTest.ExecuteAsync(referral.Id);

            // Assert
            referral.Status.Should().Be(ReferralStatus.Approved);
            referral.ServiceElements.Count.Should().Be(1);
            referral.CareChargeStatus.Should().Be(CareChargeStatus.Suspension);

            _mockDbSaver.VerifyChangesSaved();
        }

        [Test]
        public async Task ThrowsArgumentNullExceptionWhenReferralNotFound()
        {
            const int unknownReferralId = 1234;

            Func<Task> act = () => _classUnderTest.ExecuteAsync(unknownReferralId);

            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithMessage($"Referral not found for: {unknownReferralId} (Parameter 'referralId')");
        }

        [Test]
        public async Task ThrowsArgumentNullExceptionWhenCarePackageNotFound()
        {
            const int expectedReferralId = 1234;
            _mockReferralGateway
                .Setup(x => x.GetByIdWithElementsAsync(expectedReferralId))
                .ReturnsAsync(new Referral());

            Func<Task> act = () => _classUnderTest.ExecuteAsync(expectedReferralId);

            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithMessage($"Care package not found for: {expectedReferralId} (Parameter 'referralId')");
        }

        [Test]
        public async Task ThrowsUnauthorizedWhenUserApprovalLimitTooLow()
        {
            var (referral, carePackage) = SetupReferralAndCarePackage(ReferralStatus.AwaitingApproval, 1000);

            SetupUser(carePackage.EstimatedYearlyCost - 10);

            Func<Task> act = () => _classUnderTest.ExecuteAsync(referral.Id);

            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("Approver does not have high enough approval limit");
        }

        [Test]
        public async Task ThrowsInvalidOperationWhenCarePackageNotAwaitingApproval([Values] ReferralStatus status)
        {
            var (referral, carePackage) = SetupReferralAndCarePackage(status, 1000);

            SetupUser(carePackage.EstimatedYearlyCost + 10);

            Func<Task> act = () => _classUnderTest.ExecuteAsync(referral.Id);

            if (status != ReferralStatus.AwaitingApproval)
            {
                await act.Should().ThrowAsync<InvalidOperationException>()
                    .WithMessage("Referral is not in a valid state for approval");
            }
        }

        [Test]
        public async Task AddsApprovedAuditEvent()
        {
            var elements = _fixture.BuildElement(1, 1)
                .With(e => e.InternalStatus, ElementStatus.AwaitingApproval)
                .CreateMany();

            var (referral, carePackage) = SetupReferralAndCarePackage(ReferralStatus.AwaitingApproval, 1000, elements.ToArray());

            var expectedUser = SetupUser(carePackage.EstimatedYearlyCost + 100);

            await _classUnderTest.ExecuteAsync(referral.Id);

            _mockAuditGateway.VerifyAuditEventAdded(AuditEventType.CarePackageApproved);
            _mockAuditGateway.LastUserId.Should().Be(expectedUser.Id);
            _mockAuditGateway.LastSocialCareId.Should().Be(referral.SocialCareId);
            var eventMetadata = _mockAuditGateway.LastMetadata.Should().BeOfType<CarePackageApprovalAuditEventMetadata>().Which;
            eventMetadata.ReferralId.Should().Be(referral.Id);
        }

        private static readonly object[] _pendingStates =
        {
            new object[] // pending ending
            {
                LocalDate.FromDateTime(DateTime.Today), "expected comment here", null, AuditEventType.ElementEnded
            },
            new object[] // pending cancellation
            {
                null, "expected comment here", true, AuditEventType.ElementCancelled
            },
        };

        [TestCaseSource(nameof(_pendingStates)), Property("AsUser", "Broker")]
        public async Task TransfersPendingStates(
            LocalDate? expectedEndDate,
            [CanBeNull] string expectedComment,
            bool? expectedCancellation,
            AuditEventType auditEventType
        )
        {
            var referralElement = _fixture.BuildReferralElement(1, 1)
                .With(re => re.PendingEndDate, expectedEndDate)
                .With(re => re.PendingComment, expectedComment)
                .With(re => re.PendingCancellation, expectedCancellation)
                .Create();


            var element = _fixture.BuildElement(1, 1)
                .With(e => e.InternalStatus, ElementStatus.AwaitingApproval)
                .Without(e => e.EndDate)
                .Create();

            var referral = _fixture.BuildReferral(ReferralStatus.AwaitingApproval)
                .With(r => r.Elements, new List<Element>
                {
                    element
                })
                .Create();

            referralElement.ElementId = element.Id;
            referralElement.ReferralId = referral.Id;
            element.ReferralElements = new List<ReferralElement>
            {
                referralElement
            };
            referral.ReferralElements = new List<ReferralElement>
            {
                referralElement
            };

            _mockReferralGateway
                .Setup(x => x.GetByIdWithElementsAsync(referral.Id))
                .ReturnsAsync(referral);

            _mockCarePackageGateway
                .Setup(x => x.GetByIdAsync(referral.Id))
                .ReturnsAsync(new CarePackage
                {
                    Id = referral.Id,
                    Status = referral.Status
                });

            var expectedUser = SetupUser(element.Cost * 100);

            await _classUnderTest.ExecuteAsync(referral.Id);

            element.EndDate.Should().Be(expectedEndDate);
            element.Comment.Should().Be(expectedComment);

            if (expectedCancellation.HasValue && expectedCancellation.Value)
            {
                element.InternalStatus.Should().Be(ElementStatus.Cancelled);
                referralElement.PendingCancellation.Should().BeFalse();
            }

            referralElement.PendingEndDate.Should().BeNull();
            referralElement.PendingComment.Should().BeNull();

            _mockDbSaver.VerifyChangesSaved();

            _mockAuditGateway.VerifyAuditEventAdded(auditEventType);

            var auditEventCall = _mockAuditGateway.AllCalls.Single(c => c.type == auditEventType);

            auditEventCall.userId.Should().Be(expectedUser.Id);
            auditEventCall.socialCareId.Should().Be(referral.SocialCareId);

            var eventMetadata = auditEventCall.metadata.Should().BeOfType<ElementAuditEventMetadata>().Which;
            eventMetadata.ReferralId.Should().Be(referral.Id);
            eventMetadata.ElementId.Should().Be(element.Id);
            eventMetadata.ElementDetails.Should().Be(element.Details);
            eventMetadata.Comment.Should().Be(expectedComment);
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
        private (Referral referral, CarePackage carePackage) SetupReferralAndCarePackage(ReferralStatus status, decimal estimatedYearlyCost = 0, params Element[] elements)
        {
            var referralBuilder = _fixture.BuildReferral(status);

            if (elements.Length > 0)
            {
                referralBuilder = referralBuilder.With(r => r.Elements, elements.ToList);
            }

            var referral = referralBuilder.Create();

            var carePackage = _fixture.BuildCarePackage()
                .With(c => c.Id, referral.Id)
                .With(c => c.EstimatedYearlyCost, estimatedYearlyCost)
                .With(c => c.Status, referral.Status)
                .Create();

            _mockReferralGateway
                .Setup(x => x.GetByIdWithElementsAsync(referral.Id))
                .ReturnsAsync(referral);

            _mockCarePackageGateway
                .Setup(x => x.GetByIdAsync(referral.Id))
                .ReturnsAsync(carePackage);

            return (referral, carePackage);
        }
    }
}
