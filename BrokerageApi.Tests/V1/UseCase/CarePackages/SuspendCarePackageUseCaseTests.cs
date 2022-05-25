using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using BrokerageApi.Tests.V1.Helpers;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.Services.Interfaces;
using BrokerageApi.V1.UseCase.CarePackages;
using BrokerageApi.V1.UseCase.Interfaces.CarePackageElements;
using FluentAssertions;
using Moq;
using NodaTime;
using NUnit.Framework;

namespace BrokerageApi.Tests.V1.UseCase.CarePackages
{
    public class SuspendCarePackageUseCaseTests
    {
        private Fixture _fixture;
        private Mock<IReferralGateway> _mockReferralsGateway;
        private SuspendCarePackageUseCase _classUnderTest;
        private Mock<ISuspendElementUseCase> _mockSuspendElementUseCase;
        private Mock<IClockService> _mockClock;
        private Instant _currentInstance;
        private MockDbSaver _mockDbSaver;

        [SetUp]
        public void Setup()
        {
            _fixture = FixtureHelpers.Fixture;
            _mockReferralsGateway = new Mock<IReferralGateway>();
            _mockSuspendElementUseCase = new Mock<ISuspendElementUseCase>();
            _mockDbSaver = new MockDbSaver();
            _mockClock = new Mock<IClockService>();
            _currentInstance = SystemClock.Instance.GetCurrentInstant();
            _mockClock.Setup(x => x.Now)
                .Returns(_currentInstance);

            _classUnderTest = new SuspendCarePackageUseCase(
                _mockReferralsGateway.Object,
                _mockSuspendElementUseCase.Object,
                _mockDbSaver.Object,
                _mockClock.Object
            );
        }

        [Test]
        public async Task CanEndCarePackage()
        {
            var startDate = LocalDate.FromDateTime(DateTime.Today);
            var endDate = startDate.PlusDays(2);
            var elements = _fixture.BuildElement(1, 1)
                .With(e => e.InternalStatus, ElementStatus.Approved)
                .With(e => e.StartDate, startDate.PlusDays(-5))
                .With(e => e.EndDate, endDate.PlusDays(5))
                .CreateMany();

            var referral = _fixture.BuildReferral(ReferralStatus.Approved)
                .With(r => r.Elements, elements.ToList())
                .Create();

            _mockReferralsGateway.Setup(x => x.GetByIdWithElementsAsync(referral.Id))
                .ReturnsAsync(referral);

            await _classUnderTest.ExecuteAsync(referral.Id, startDate, endDate);

            foreach (var element in elements)
            {
                _mockSuspendElementUseCase.Verify(x => x.ExecuteAsync(referral.Id, element.Id, startDate, endDate), Times.Once);
            }

            referral.UpdatedAt.Should().Be(_currentInstance);
            _mockDbSaver.VerifyChangesSaved();
        }

        [Test]
        public async Task ThrowsArgumentNullExceptionWhenReferralNotFound()
        {
            var startDate = LocalDate.FromDateTime(DateTime.Today);
            var endDate = startDate.PlusDays(2);
            var unknownReferralId = 1234;
            _mockReferralsGateway.Setup(x => x.GetByIdAsync(unknownReferralId))
                .ReturnsAsync((Referral) null);

            Func<Task> act = () => _classUnderTest.ExecuteAsync(unknownReferralId, startDate, endDate);

            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithMessage($"Referral not found for: {unknownReferralId} (Parameter 'referralId')");
            _mockSuspendElementUseCase.VerifyNoOtherCalls();
            _mockDbSaver.VerifyChangesNotSaved();
        }
    }
}
