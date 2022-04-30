using System;
using System.Threading.Tasks;
using AutoFixture;
using BrokerageApi.Tests.V1.Helpers;
using BrokerageApi.V1.Boundary.Request;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.Services.Interfaces;
using BrokerageApi.V1.UseCase;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NodaTime;

namespace BrokerageApi.Tests.V1.UseCase
{
    [TestFixture]
    public class StartCarePackageUseCaseTests
    {
        private StartCarePackageUseCase _classUnderTest;
        private Fixture _fixture;
        private Mock<IReferralGateway> _referralGatewayMock;
        private Mock<IUserService> _userServiceMock;
        private Mock<IClockService> _clockMock;
        private Mock<IDbSaver> _dbSaverMock;

        [SetUp]
        public void Setup()
        {
            _fixture = FixtureHelpers.Fixture;
            _referralGatewayMock = new Mock<IReferralGateway>();
            _userServiceMock = new Mock<IUserService>();
            _clockMock = new Mock<IClockService>();
            _dbSaverMock = new Mock<IDbSaver>();

            _classUnderTest = new StartCarePackageUseCase(
                _referralGatewayMock.Object,
                _userServiceMock.Object,
                _clockMock.Object,
                _dbSaverMock.Object
            );
        }

        [Test]
        public async Task StartsCarePackage()
        {
            // Arrange
            var currentInstant = SystemClock.Instance.GetCurrentInstant();

            var referral = _fixture.Build<Referral>()
                .With(x => x.Status, ReferralStatus.Assigned)
                .With(x => x.AssignedTo, "a.broker@hackney.gov.uk")
                .Without(x => x.StartedAt)
                .Create();

            _referralGatewayMock
                .Setup(x => x.GetByIdAsync(referral.Id))
                .ReturnsAsync(referral);

            _userServiceMock
                .SetupGet(x => x.Name)
                .Returns("a.broker@hackney.gov.uk");

            _clockMock
                .SetupGet(x => x.Now)
                .Returns(currentInstant);

            _dbSaverMock
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _classUnderTest.ExecuteAsync(referral.Id);

            // Assert
            Assert.That(result.Status, Is.EqualTo(ReferralStatus.InProgress));
            Assert.That(result.StartedAt, Is.EqualTo(currentInstant));
            _dbSaverMock.Verify(x => x.SaveChangesAsync(), Times.Once());
        }

        [Test]
        public void ThrowsArgumentNullExceptionWhenReferralDoesntExist()
        {
            // Arrange
            _referralGatewayMock
                .Setup(x => x.GetByIdAsync(123456))
                .ReturnsAsync(null as Referral);

            _userServiceMock
                .SetupGet(x => x.Name)
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

            _referralGatewayMock
                .Setup(x => x.GetByIdAsync(referral.Id))
                .ReturnsAsync(referral);

            _userServiceMock
                .SetupGet(x => x.Name)
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
                .With(x => x.AssignedTo, "other.broker@hackney.gov.uk")
                .Create();

            _referralGatewayMock
                .Setup(x => x.GetByIdAsync(referral.Id))
                .ReturnsAsync(referral);

            _userServiceMock
                .SetupGet(x => x.Name)
                .Returns("a.broker@hackney.gov.uk");

            // Act
            var exception = Assert.ThrowsAsync<UnauthorizedAccessException>(
                async () => await _classUnderTest.ExecuteAsync(referral.Id));

            // Assert
            Assert.That(exception.Message, Is.EqualTo("Referral is not assigned to a.broker@hackney.gov.uk"));
        }
    }
}
