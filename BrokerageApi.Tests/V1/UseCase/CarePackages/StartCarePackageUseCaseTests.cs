using System;
using System.Threading.Tasks;
using AutoFixture;
using BrokerageApi.Tests.V1.Helpers;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.Services.Interfaces;
using BrokerageApi.V1.UseCase.CarePackages;
using Moq;
using NodaTime;
using NUnit.Framework;

namespace BrokerageApi.Tests.V1.UseCase.CarePackages
{
    [TestFixture]
    public class StartCarePackageUseCaseTests
    {
        private StartCarePackageUseCase _classUnderTest;
        private Fixture _fixture;
        private Mock<IReferralGateway> _mockReferralGateway;
        private Mock<IUserService> _mockUserService;
        private Mock<IClockService> _mockClock;
        private Mock<IDbSaver> _mockDbSaver;

        [SetUp]
        public void Setup()
        {
            _fixture = FixtureHelpers.Fixture;
            _mockReferralGateway = new Mock<IReferralGateway>();
            _mockUserService = new Mock<IUserService>();
            _mockClock = new Mock<IClockService>();
            _mockDbSaver = new Mock<IDbSaver>();

            _classUnderTest = new StartCarePackageUseCase(
                _mockReferralGateway.Object,
                _mockUserService.Object,
                _mockClock.Object,
                _mockDbSaver.Object
            );
        }

        [Test]
        public async Task StartsCarePackage()
        {
            // Arrange
            var currentInstant = SystemClock.Instance.GetCurrentInstant();

            var referral = _fixture.Build<Referral>()
                .With(x => x.Status, ReferralStatus.Assigned)
                .With(x => x.AssignedBroker, "a.broker@hackney.gov.uk")
                .Without(x => x.StartedAt)
                .Create();

            _mockReferralGateway
                .Setup(x => x.GetByIdAsync(referral.Id))
                .ReturnsAsync(referral);

            _mockUserService
                .SetupGet(x => x.Email)
                .Returns("a.broker@hackney.gov.uk");

            _mockClock
                .SetupGet(x => x.Now)
                .Returns(currentInstant);

            _mockDbSaver
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _classUnderTest.ExecuteAsync(referral.Id);

            // Assert
            Assert.That(result.Status, Is.EqualTo(ReferralStatus.InProgress));
            Assert.That(result.StartedAt, Is.EqualTo(currentInstant));
            _mockDbSaver.Verify(x => x.SaveChangesAsync(), Times.Once());
        }

        [Test]
        public void ThrowsArgumentNullExceptionWhenReferralDoesntExist()
        {
            // Arrange
            _mockReferralGateway
                .Setup(x => x.GetByIdAsync(123456))
                .ReturnsAsync(null as Referral);

            _mockUserService
                .SetupGet(x => x.Email)
                .Returns("a.broker@hackney.gov.uk");

            // Act
            var exception = Assert.ThrowsAsync<ArgumentNullException>(
                async () => await _classUnderTest.ExecuteAsync(123456));

            // Assert
            Assert.That(exception.Message, Is.EqualTo("Referral not found for: 123456 (Parameter 'referralId')"));
        }

        [Test]
        public void ThrowsInvalidOperationExceptionWhenReferralIsUnassigned()
        {
            // Arrange
            var referral = _fixture.Build<Referral>()
                .With(x => x.Status, ReferralStatus.Unassigned)
                .Create();

            _mockReferralGateway
                .Setup(x => x.GetByIdAsync(referral.Id))
                .ReturnsAsync(referral);

            _mockUserService
                .SetupGet(x => x.Email)
                .Returns("a.broker@hackney.gov.uk");

            // Act
            var exception = Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _classUnderTest.ExecuteAsync(referral.Id));

            // Assert
            Assert.That(exception.Message, Is.EqualTo("Referral is not in a valid state to start editing"));
        }

        [Test]
        public void ThrowsUnauthorizedAccessExceptionWhenReferralIsAssignedToSomeoneElse()
        {
            // Arrange
            var referral = _fixture.Build<Referral>()
                .With(x => x.Status, ReferralStatus.Assigned)
                .With(x => x.AssignedBroker, "other.broker@hackney.gov.uk")
                .Create();

            _mockReferralGateway
                .Setup(x => x.GetByIdAsync(referral.Id))
                .ReturnsAsync(referral);

            _mockUserService
                .SetupGet(x => x.Email)
                .Returns("a.broker@hackney.gov.uk");

            // Act
            var exception = Assert.ThrowsAsync<UnauthorizedAccessException>(
                async () => await _classUnderTest.ExecuteAsync(referral.Id));

            // Assert
            Assert.That(exception.Message, Is.EqualTo("Referral is not assigned to a.broker@hackney.gov.uk"));
        }
    }
}
