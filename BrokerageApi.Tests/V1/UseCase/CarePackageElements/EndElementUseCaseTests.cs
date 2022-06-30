using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using BrokerageApi.Tests.V1.Helpers;
using BrokerageApi.Tests.V1.UseCase.Mocks;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;
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
    public class EndElementUseCaseTests
    {
        private Fixture _fixture;
        private Mock<IElementGateway> _mockElementGateway;
        private EndElementUseCase _classUnderTest;
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
            _mockElementGateway = new Mock<IElementGateway>();
            _mockAuditGateway = new MockAuditGateway();
            _mockUserService = new Mock<IUserService>();
            _dbSaver = new MockDbSaver();

            var currentTime = SystemClock.Instance.GetCurrentInstant();
            var fakeClock = new FakeClock(currentTime);
            _clock = new ClockService(fakeClock);

            _classUnderTest = new EndElementUseCase(
                _mockReferralGateway.Object,
                _mockElementGateway.Object,
                _mockAuditGateway.Object,
                _mockUserService.Object,
                _dbSaver.Object,
                _clock
            );
        }

        [Test]
        public async Task CanEndElementWithoutEndDate()
        {
            var endDate = LocalDate.FromDateTime(DateTime.Today);
            var (referral, element) = CreateReferralAndElement();
            var elementUpdatedAt = element.UpdatedAt;
            _mockElementGateway.Setup(x => x.GetByIdAsync(element.Id))
                .ReturnsAsync(element);

            await _classUnderTest.ExecuteAsync(referral.Id, element.Id, endDate);

            element.EndDate.Should().BeNull();
            element.UpdatedAt.Should().Be(elementUpdatedAt);

            var referralElement = element.ReferralElements.Single(re => re.ElementId == element.Id);
            referralElement.PendingEndDate.Should().Be(endDate);

            _dbSaver.VerifyChangesSaved();
        }

        [Test]
        public async Task CanEndElementWithEndDate()
        {
            var endDate = LocalDate.FromDateTime(DateTime.Today);
            var (referral, element) = CreateReferralAndElement(ElementStatus.Approved, endDate.PlusDays(5));
            var elementEndDate = element.EndDate;
            var elementUpdatedAt = element.UpdatedAt;
            var elementComment = element.Comment;

            await _classUnderTest.ExecuteAsync(referral.Id, element.Id, endDate);

            element.EndDate.Should().Be(elementEndDate);
            element.UpdatedAt.Should().Be(elementUpdatedAt);

            var referralElement = element.ReferralElements.Single(re => re.ElementId == element.Id);
            referralElement.PendingEndDate.Should().Be(endDate);
            _dbSaver.VerifyChangesSaved();
        }

        [Test]
        public async Task ThrowsArgumentNullExceptionWhenReferralNotFound()
        {
            var endDate = LocalDate.FromDateTime(DateTime.Today);
            var unknownReferralId = 1234;
            var unknownElementId = 1234;
            _mockReferralGateway.Setup(x => x.GetByIdAsync(unknownReferralId))
                .ReturnsAsync((Referral) null);
            _mockElementGateway.Setup(x => x.GetByIdAsync(unknownElementId))
                .ReturnsAsync((Element) null);

            Func<Task> act = () => _classUnderTest.ExecuteAsync(unknownReferralId, unknownElementId, endDate);

            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithMessage($"Referral not found {unknownReferralId} (Parameter 'referralId')");
            _dbSaver.VerifyChangesNotSaved();
        }

        [Test]
        public async Task ThrowsArgumentNullExceptionWhenElementNotFound()
        {
            var endDate = LocalDate.FromDateTime(DateTime.Today);
            var (referral, _) = CreateReferralAndElement();
            var unknownElementId = 1234;
            _mockElementGateway.Setup(x => x.GetByIdAsync(unknownElementId))
                .ReturnsAsync((Element) null);

            Func<Task> act = () => _classUnderTest.ExecuteAsync(referral.Id, unknownElementId, endDate);

            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithMessage($"Element not found {unknownElementId} (Parameter 'elementId')");
            _dbSaver.VerifyChangesNotSaved();
        }

        [Test]
        public async Task ThrowsInvalidOperationWhenElementNotApproved([Values] ElementStatus status)
        {
            var endDate = LocalDate.FromDateTime(DateTime.Today);
            var (referral, element) = CreateReferralAndElement(status);

            Func<Task> act = () => _classUnderTest.ExecuteAsync(referral.Id, element.Id, endDate);

            if (status != ElementStatus.Approved)
            {
                await act.Should().ThrowAsync<InvalidOperationException>()
                    .WithMessage($"Element {element.Id} is not approved");
                _dbSaver.VerifyChangesNotSaved();
            }
        }

        [Test]
        public async Task ThrowsWhenElementEndDateIsBeforeRequested()
        {
            var endDate = LocalDate.FromDateTime(DateTime.Today);
            var (referral, element) = CreateReferralAndElement(ElementStatus.Approved, endDate.PlusDays(-5));

            Func<Task> act = () => _classUnderTest.ExecuteAsync(referral.Id, element.Id, endDate);

            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage($"Element {element.Id} has an end date before the requested end date");
            _dbSaver.VerifyChangesNotSaved();
        }

        private (Referral referral, Element element) CreateReferralAndElement(ElementStatus status = ElementStatus.Approved, LocalDate? endDate = null)
        {
            var builder = _fixture.BuildElement(1, 1)
                .With(e => e.InternalStatus, status)
                .Without(e => e.EndDate);

            if (endDate != null)
            {
                builder = builder.With(e => e.EndDate, endDate);
            }

            var element = builder.Create();

            var referral = _fixture.BuildReferral(ReferralStatus.InProgress)
                .With(r => r.Elements, new List<Element>
                {
                    element
                })
                .Create();

            element.ReferralElements = new List<ReferralElement>
            {
                new ReferralElement
                {
                    ElementId = element.Id, ReferralId = referral.Id
                }
            };

            _mockReferralGateway.Setup(x => x.GetByIdAsync(referral.Id))
                .ReturnsAsync(referral);
            _mockElementGateway.Setup(x => x.GetByIdAsync(element.Id))
                .ReturnsAsync(element);

            return (referral, element);
        }
    }
}
