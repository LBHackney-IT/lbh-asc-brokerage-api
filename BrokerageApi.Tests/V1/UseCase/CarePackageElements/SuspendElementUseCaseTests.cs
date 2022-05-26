using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using BrokerageApi.Tests.V1.Helpers;
using BrokerageApi.Tests.V1.UseCase.Mocks;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.Infrastructure.AuditEvents;
using BrokerageApi.V1.Services;
using BrokerageApi.V1.Services.Interfaces;
using BrokerageApi.V1.UseCase.CarePackageElements;
using FluentAssertions;
using Moq;
using NodaTime;
using NodaTime.Testing;
using NUnit.Framework;

namespace BrokerageApi.Tests.V1.UseCase.CarePackageElements
{
    public class SuspendElementUseCaseTests
    {
        private Fixture _fixture;
        private SuspendElementUseCase _classUnderTest;
        private MockDbSaver _dbSaver;
        private ClockService _clock;
        private Mock<IReferralGateway> _mockReferralGateway;
        private MockAuditGateway _mockAuditGateway;
        private Mock<IUserService> _mockUserService;

        [SetUp]
        public void Setup()
        {
            _fixture = FixtureHelpers.Fixture;
            _mockReferralGateway = new Mock<IReferralGateway>();
            _mockAuditGateway = new MockAuditGateway();
            _mockUserService = new Mock<IUserService>();
            _dbSaver = new MockDbSaver();

            var currentTime = SystemClock.Instance.GetCurrentInstant();
            var fakeClock = new FakeClock(currentTime);
            _clock = new ClockService(fakeClock);

            _classUnderTest = new SuspendElementUseCase(
                _mockReferralGateway.Object,
                _mockAuditGateway.Object,
                _mockUserService.Object,
                _dbSaver.Object,
                _clock
            );
        }

        [Test]
        public async Task CanSuspendElement([Values] bool withEndDate)
        {
            var baseDate = LocalDate.FromDateTime(DateTime.Today);
            var startDate = baseDate.PlusDays(1);
            var endDate = withEndDate ? startDate.PlusDays(8) : (LocalDate?) null;
            var (referral, element) = CreateReferralAndElement(ElementStatus.Approved, baseDate, baseDate.PlusDays(10));

            await _classUnderTest.ExecuteAsync(referral.Id, element.Id, startDate, endDate, null);

            var newElement = referral.Elements.SingleOrDefault(e => e.SuspendedElementId == element.Id);
            newElement.Should().NotBeNull();
            newElement.InternalStatus.Should().Be(ElementStatus.InProgress);
            newElement.CreatedAt.Should().Be(_clock.Now);
            newElement.UpdatedAt.Should().Be(_clock.Now);
            newElement.StartDate.Should().Be(startDate);
            newElement.EndDate.Should().Be(endDate);
            newElement.IsSuspension.Should().BeTrue();
            newElement.Should().BeEquivalentTo(element, options => options
                .Excluding(e => e.Id)
                .Excluding(e => e.StartDate)
                .Excluding(e => e.EndDate)
                .Excluding(e => e.ChildElements)
                .Excluding(e => e.DailyCosts)
                .Excluding(e => e.SuspensionElements)
                .Excluding(e => e.SuspendedElementId)
                .Excluding(e => e.InternalStatus)
                .Excluding(e => e.Status)
                .Excluding(e => e.CreatedAt)
                .Excluding(e => e.UpdatedAt)
                .Excluding(e => e.IsSuspension)
            );

            _dbSaver.VerifyChangesSaved();
        }

        [Test]
        public async Task ThrowsArgumentNullExceptionWhenReferralNotFound()
        {
            var startDate = LocalDate.FromDateTime(DateTime.Today);
            var endDate = LocalDate.FromDateTime(DateTime.Today).PlusDays(1);
            var unknownReferralId = 1234;
            var unknownElementId = 1234;
            _mockReferralGateway.Setup(x => x.GetByIdAsync(unknownReferralId))
                .ReturnsAsync((Referral) null);

            Func<Task> act = () => _classUnderTest.ExecuteAsync(unknownReferralId, unknownElementId, startDate, endDate, null);

            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithMessage($"Referral not found {unknownReferralId} (Parameter 'referralId')");
            _dbSaver.VerifyChangesNotSaved();
        }

        [Test]
        public async Task ThrowsArgumentNullExceptionWhenElementNotFound()
        {
            var startDate = LocalDate.FromDateTime(DateTime.Today);
            var endDate = LocalDate.FromDateTime(DateTime.Today).PlusDays(1);
            var (referral, _) = CreateReferralAndElement();
            var unknownElementId = 1234;

            Func<Task> act = () => _classUnderTest.ExecuteAsync(referral.Id, unknownElementId, startDate, endDate, null);

            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithMessage($"Element not found {unknownElementId} (Parameter 'elementId')");
            _dbSaver.VerifyChangesNotSaved();
        }

        [Test]
        public async Task ThrowsInvalidOperationWhenElementNotApproved([Values] ElementStatus status)
        {
            var startDate = LocalDate.FromDateTime(DateTime.Today);
            var endDate = LocalDate.FromDateTime(DateTime.Today).PlusDays(1);
            var (referral, element) = CreateReferralAndElement(status);

            Func<Task> act = () => _classUnderTest.ExecuteAsync(referral.Id, element.Id, startDate, endDate, null);

            if (status != ElementStatus.Approved)
            {
                await act.Should().ThrowAsync<InvalidOperationException>()
                    .WithMessage($"Element {element.Id} is not approved");
                _dbSaver.VerifyChangesNotSaved();
            }
        }

        [TestCase(0, 10, 1, 11, true)] // Suspend end > Element end
        [TestCase(0, 10, -1, 9, true)] // Suspend start < Element start
        [TestCase(0, 10, -1, 11, true)] // Suspend start < Element start AND Suspend end > Element end
        [TestCase(0, null, -1, 10, true)] // Element end is NULL AND Suspend start < Element start
        [TestCase(0, 10, 0, 10, false)] // Suspend start = Element start AND Suspend end = Element end (doesn't throw)
        [TestCase(0, null, 0, 10, false)] // Element end is NULL (doesn't throw)
        public async Task ThrowsWhenDatesNotWithinElementDates(int elementStartOffset, int? elementEndOffset, int suspendStartOffset, int suspendEndOffset, bool shouldThrow)
        {
            var baseDate = LocalDate.FromDateTime(DateTime.Today);
            var startDate = baseDate.PlusDays(suspendStartOffset);
            var endDate = baseDate.PlusDays(suspendEndOffset);
            var (referral, element) = CreateReferralAndElement(
                ElementStatus.Approved,
                baseDate.PlusDays(elementStartOffset),
                elementEndOffset is null ? (LocalDate?) null : baseDate.PlusDays(elementEndOffset.Value));

            Func<Task> act = () => _classUnderTest.ExecuteAsync(referral.Id, element.Id, startDate, endDate, null);

            if (shouldThrow)
            {
                await act.Should().ThrowAsync<ArgumentException>()
                    .WithMessage("Requested dates do not fall in elements dates");
                _dbSaver.VerifyChangesNotSaved();
            }
            else
            {
                await act.Should().NotThrowAsync<ArgumentException>();
                _dbSaver.VerifyChangesSaved();
            }
        }

        [Test]
        public async Task AddsAuditTrail()
        {
            const string expectedComment = "commentHere";
            var baseDate = LocalDate.FromDateTime(DateTime.Today);
            var startDate = baseDate.PlusDays(1);
            var endDate = startDate.PlusDays(8);
            var (referral, element) = CreateReferralAndElement(ElementStatus.Approved, baseDate, baseDate.PlusDays(10));
            const int expectedUserId = 1234;
            _mockUserService
                .Setup(x => x.UserId)
                .Returns(expectedUserId);

            await _classUnderTest.ExecuteAsync(referral.Id, element.Id, startDate, endDate, expectedComment);

            _mockAuditGateway.VerifyAuditEventAdded(AuditEventType.ElementSuspended);
            _mockAuditGateway.LastUserId.Should().Be(expectedUserId);
            _mockAuditGateway.LastSocialCareId.Should().Be(referral.SocialCareId);
            var eventMetadata = _mockAuditGateway.LastMetadata.Should().BeOfType<ElementAuditEventMetadata>().Which;
            eventMetadata.ReferralId.Should().Be(referral.Id);
            eventMetadata.ElementId.Should().Be(element.Id);
            eventMetadata.ElementDetails.Should().Be(element.Details);
            eventMetadata.Comment.Should().Be(expectedComment);
        }

        private (Referral referral, Element element) CreateReferralAndElement(ElementStatus status = ElementStatus.Approved, LocalDate? startDate = null, LocalDate? endDate = null)
        {
            var builder = _fixture.BuildElement(1, 1)
                .With(e => e.InternalStatus, status)
                .Without(e => e.EndDate);

            if (startDate != null)
            {
                builder = builder.With(e => e.StartDate, startDate);
            }

            if (endDate != null)
            {
                builder = builder.With(e => e.EndDate, endDate);
            }

            var elements = builder.CreateMany();

            var referral = _fixture.BuildReferral(ReferralStatus.InProgress)
                .With(r => r.Elements, elements.ToList).Create();

            _mockReferralGateway.Setup(x => x.GetByIdWithElementsAsync(referral.Id))
                .ReturnsAsync(referral);

            return (referral, elements.First());
        }
    }
}
