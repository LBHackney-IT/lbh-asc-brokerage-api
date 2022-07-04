using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using BrokerageApi.Tests.V1.Helpers;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.Services.Interfaces;
using BrokerageApi.V1.UseCase.CarePackageCareCharges;
using FluentAssertions;
using Moq;
using NodaTime;
using NUnit.Framework;

namespace BrokerageApi.Tests.V1.UseCase.CarePackageCareCharges
{
    public class ConfirmCareChargesUseCaseTests
    {
        private ConfirmCareChargesUseCase _classUnderTest;
        private Fixture _fixture;
        private Mock<IReferralGateway> _mockReferralGateway;
        private Mock<IClockService> _mockClock;
        private MockDbSaver _mockDbSaver;

        [SetUp]
        public void Setup()
        {
            _fixture = FixtureHelpers.Fixture;
            _mockReferralGateway = new Mock<IReferralGateway>();
            _mockClock = new Mock<IClockService>();
            _mockDbSaver = new MockDbSaver();

            _classUnderTest = new ConfirmCareChargesUseCase(
                _mockReferralGateway.Object,
                _mockClock.Object,
                _mockDbSaver.Object
            );
        }

        [Test]
        public async Task CanConfirmCareCharges()
        {
            // Arrange
            var currentInstant = SystemClock.Instance.GetCurrentInstant();
            var previousInstant = currentInstant - Duration.FromMinutes(60);

            var service = _fixture.BuildService()
                .Create();

            var elementType = _fixture.BuildElementType(service.Id, ElementTypeType.ConfirmedCareCharge)
                .Create();

            var element = _fixture.BuildElement(elementType.Id)
                .With(e => e.InternalStatus, ElementStatus.InProgress)
                .With(e => e.CreatedAt, previousInstant)
                .With(e => e.UpdatedAt, previousInstant)
                .Create();

            var referral = _fixture.BuildReferral(ReferralStatus.Approved)
                .Without(r => r.CareChargesConfirmedAt)
                .With(r => r.CreatedAt, previousInstant)
                .With(r => r.UpdatedAt, previousInstant)
                .With(r => r.Elements, new List<Element> { element })
                .Create();

            _mockReferralGateway.Setup(x => x.GetByIdWithElementsAsync(referral.Id))
                .ReturnsAsync(referral);

            _mockClock.SetupGet(x => x.Now)
                .Returns(currentInstant);

            // Act
            await _classUnderTest.ExecuteAsync(referral.Id);

            // Assert
            referral.Status.Should().Be(ReferralStatus.Approved);
            referral.CareChargesConfirmedAt.Should().Be(currentInstant);
            referral.CreatedAt.Should().Be(previousInstant);
            referral.UpdatedAt.Should().Be(currentInstant);

            element.InternalStatus.Should().Be(ElementStatus.Approved);
            element.CreatedAt.Should().Be(previousInstant);
            element.UpdatedAt.Should().Be(currentInstant);

            _mockDbSaver.VerifyChangesSaved();
        }

        [Test]
        public async Task ThrowsArgumentNullExceptionWhenReferralNotFound()
        {
            // Arrange
            _mockReferralGateway.Setup(x => x.GetByIdWithElementsAsync(1234))
                .ReturnsAsync((Referral) null);

            // Act
            var act = () => _classUnderTest.ExecuteAsync(1234);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithMessage("Referral not found for: 1234 (Parameter 'referralId')");

            _mockDbSaver.VerifyChangesNotSaved();
        }

        [Test]
        public async Task ThrowsInvalidOperationWhenReferralNotApproved([Values] ReferralStatus status)
        {
            // Arrange
            var referral = _fixture.BuildReferral(status)
                .Create();

            _mockReferralGateway.Setup(x => x.GetByIdWithElementsAsync(referral.Id))
                .ReturnsAsync(referral);

            // Act
            var act = () => _classUnderTest.ExecuteAsync(referral.Id);

            // Assert
            if (status != ReferralStatus.Approved)
            {
                await act.Should().ThrowAsync<InvalidOperationException>()
                    .WithMessage("Referral is not in a valid state for confirming care charges");

                _mockDbSaver.VerifyChangesNotSaved();
            }
        }

        [Test]
        public async Task ThrowsInvalidOperationWhenReferralAlreadyConfirmed()
        {
            // Arrange
            var currentInstant = SystemClock.Instance.GetCurrentInstant();

            var referral = _fixture.BuildReferral(ReferralStatus.Approved)
                .With(r => r.CareChargesConfirmedAt, currentInstant)
                .Create();

            _mockReferralGateway.Setup(x => x.GetByIdWithElementsAsync(referral.Id))
                .ReturnsAsync(referral);

            // Act
            var act = () => _classUnderTest.ExecuteAsync(referral.Id);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Charges have already been confirmed for this care package");

            _mockDbSaver.VerifyChangesNotSaved();
        }
    }
}
