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
    public class EndCarePackageUseCaseTests
    {
        private Fixture _fixture;
        private Mock<IReferralGateway> _mockReferralsGateway;
        private EndCarePackageUseCase _classUnderTest;
        private Mock<IEndElementUseCase> _mockEndElementUseCase;
        private MockDbSaver _mockDbSaver;
        private Mock<IClockService> _mockClock;
        private Instant _currentInstance;

        [SetUp]
        public void Setup()
        {
            _fixture = FixtureHelpers.Fixture;
            _mockReferralsGateway = new Mock<IReferralGateway>();
            _mockEndElementUseCase = new Mock<IEndElementUseCase>();
            _mockDbSaver = new MockDbSaver();
            _mockClock = new Mock<IClockService>();
            _currentInstance = SystemClock.Instance.GetCurrentInstant();
            _mockClock.Setup(x => x.Now)
                .Returns(_currentInstance);

            _classUnderTest = new EndCarePackageUseCase(
                _mockReferralsGateway.Object,
                _mockEndElementUseCase.Object,
                _mockDbSaver.Object,
                _mockClock.Object
            );
        }

        [Test]
        public async Task CanEndCarePackage()
        {
            var baseDate = LocalDate.FromDateTime(DateTime.Today);
            var elements = _fixture.BuildElement(1, 1)
                .With(e => e.InternalStatus, ElementStatus.Approved)
                .With(e => e.StartDate, baseDate.PlusDays(-5))
                .With(e => e.EndDate, baseDate.PlusDays(5))
                .CreateMany();

            var referral = _fixture.BuildReferral(ReferralStatus.Approved)
                .With(r => r.Elements, elements.ToList())
                .Create();

            _mockReferralsGateway.Setup(x => x.GetByIdWithElementsAsync(referral.Id))
                .ReturnsAsync(referral);

            await _classUnderTest.ExecuteAsync(referral.Id, baseDate);

            foreach (var element in elements)
            {
                _mockEndElementUseCase.Verify(x => x.ExecuteAsync(referral.Id, element.Id, baseDate), Times.Once);
            }
            referral.UpdatedAt.Should().Be(_currentInstance);
            _mockDbSaver.VerifyChangesSaved();
        }

        [Test]
        public async Task ThrowsArgumentNullExceptionWhenReferralNotFound()
        {
            var baseDate = LocalDate.FromDateTime(DateTime.Today);
            var unknownReferralId = 1234;
            _mockReferralsGateway.Setup(x => x.GetByIdAsync(unknownReferralId))
                .ReturnsAsync((Referral) null);

            Func<Task> act = () => _classUnderTest.ExecuteAsync(unknownReferralId, baseDate);

            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithMessage($"Referral not found for: {unknownReferralId} (Parameter 'referralId')");
            _mockEndElementUseCase.VerifyNoOtherCalls();
            _mockDbSaver.VerifyChangesNotSaved();
        }
    }
}
