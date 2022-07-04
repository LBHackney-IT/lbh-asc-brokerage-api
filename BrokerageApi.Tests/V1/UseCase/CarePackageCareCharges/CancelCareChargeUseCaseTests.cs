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
using BrokerageApi.V1.UseCase.CarePackageCareCharges;
using FluentAssertions;
using Moq;
using NodaTime;
using NodaTime.Testing;
using NUnit.Framework;

namespace BrokerageApi.Tests.V1.UseCase.CarePackageCareCharges
{
    public class CancelCareChargeUseCaseTests
    {
        private Fixture _fixture;
        private Mock<IElementGateway> _mockElementGateway;
        private CancelCareChargeUseCase _classUnderTest;
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

            _classUnderTest = new CancelCareChargeUseCase(
                _mockReferralGateway.Object,
                _mockElementGateway.Object,
                _mockAuditGateway.Object,
                _mockUserService.Object,
                _dbSaver.Object,
                _clock
            );
        }

        [Test]
        public async Task CanCancelCareCharge()
        {
            const string expectedComment = "commentHere";
            var (referral, element) = CreateReferralAndElement();
            var elementStatus = element.InternalStatus;
            var elementUpdatedAt = element.UpdatedAt;
            var elementComment = element.Comment;

            _mockElementGateway.Setup(x => x.GetByIdAsync(element.Id))
                .ReturnsAsync(element);

            await _classUnderTest.ExecuteAsync(referral.Id, element.Id, expectedComment);

            element.InternalStatus.Should().Be(elementStatus);
            element.UpdatedAt.Should().Be(elementUpdatedAt);
            element.Comment.Should().Be(elementComment);

            var referralElement = element.ReferralElements.Single(re => re.ElementId == element.Id);
            referralElement.PendingCancellation.Should().BeTrue();
            referralElement.PendingComment.Should().Be(expectedComment);

            _dbSaver.VerifyChangesSaved();
        }

        [Test]
        public async Task ThrowsArgumentNullExceptionWhenReferralNotFound()
        {
            var unknownReferralId = 1234;
            var unknownElementId = 1234;
            _mockReferralGateway.Setup(x => x.GetByIdAsync(unknownReferralId))
                .ReturnsAsync((Referral) null);
            _mockElementGateway.Setup(x => x.GetByIdAsync(unknownElementId))
                .ReturnsAsync((Element) null);

            var act = () => _classUnderTest.ExecuteAsync(unknownReferralId, unknownElementId, null);

            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithMessage($"Referral not found {unknownReferralId} (Parameter 'referralId')");
            _dbSaver.VerifyChangesNotSaved();
        }

        [Test]
        public async Task ThrowsArgumentNullExceptionWhenElementNotFound()
        {
            var (referral, _) = CreateReferralAndElement();
            var unknownElementId = 1234;
            _mockElementGateway.Setup(x => x.GetByIdAsync(unknownElementId))
                .ReturnsAsync((Element) null);

            var act = () => _classUnderTest.ExecuteAsync(referral.Id, unknownElementId, null);

            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithMessage($"Element not found {unknownElementId} (Parameter 'elementId')");
            _dbSaver.VerifyChangesNotSaved();
        }

        [Test]
        public async Task ThrowsInvalidOperationWhenElementNotApproved([Values] ElementStatus status)
        {
            var (referral, element) = CreateReferralAndElement(status);

            var act = () => _classUnderTest.ExecuteAsync(referral.Id, element.Id, null);

            if (status != ElementStatus.Approved)
            {
                await act.Should().ThrowAsync<InvalidOperationException>()
                    .WithMessage($"Element {element.Id} is not approved");
                _dbSaver.VerifyChangesNotSaved();
            }
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

            var referral = _fixture.BuildReferral(ReferralStatus.Approved)
                .With(r => r.Elements, new List<Element>
                {
                    element
                }).Create();

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
