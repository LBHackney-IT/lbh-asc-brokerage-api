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
        private Mock<IClockService> _clockMock;
        private Mock<IDbSaver> _dbSaverMock;

        [SetUp]
        public void Setup()
        {
            _fixture = FixtureHelpers.Fixture;
            _referralGatewayMock = new Mock<IReferralGateway>();
            _clockMock = new Mock<IClockService>();
            _dbSaverMock = new Mock<IDbSaver>();

            _classUnderTest = new StartCarePackageUseCase(
                _referralGatewayMock.Object,
                _clockMock.Object,
                _dbSaverMock.Object
            );
        }

        [Test]
        public async Task StartsCarePackage()
        {
            // Arrange
            var currentInstant = SystemClock.Instance.GetCurrentInstant();
            var assignedUser = "a.broker@hackney.gov.uk";

            var referral = _fixture.Build<Referral>()
                .With(x => x.Status, ReferralStatus.Assigned)
                .With(x => x.AssignedTo, assignedUser)
                .Without(x => x.StartedAt)
                .Create();

            _referralGatewayMock.Setup(x => x.GetByIdAsync(referral.Id))
                .ReturnsAsync(referral);

            _clockMock.SetupGet(x => x.Now)
                .Returns(currentInstant);

            _dbSaverMock.Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _classUnderTest.ExecuteAsync(referral.Id, assignedUser);

            // Assert
            Assert.That(result.Status, Is.EqualTo(ReferralStatus.InProgress));
            Assert.That(result.StartedAt, Is.EqualTo(currentInstant));
            _dbSaverMock.Verify(x => x.SaveChangesAsync(), Times.Once());
        }

        [Test]
        public void ThrowsArgumentExceptionWhenReferralDoesntExist()
        {
            // Arrange
            _referralGatewayMock.Setup(x => x.GetByIdAsync(123456))
                .ReturnsAsync(null as Referral);

            // Act
            var exception = Assert.ThrowsAsync<ArgumentException>(
                async () => await _classUnderTest.ExecuteAsync(123456, "a.broker@hackney.gov.uk"));

            // Assert
            Assert.That(exception.Message, Is.EqualTo("Referral not found for: 123456"));
        }

        [Test]
        public void ThrowsInvalidOperationExceptionWhenReferralIsUnassigned()
        {
            // Arrange
            var referral = _fixture.Build<Referral>()
                .With(x => x.Status, ReferralStatus.Unassigned)
                .Create();

            _referralGatewayMock.Setup(x => x.GetByIdAsync(referral.Id))
                .ReturnsAsync(referral);

            // Act
            var exception = Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _classUnderTest.ExecuteAsync(referral.Id, "a.broker@hackney.gov.uk"));

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

            _referralGatewayMock.Setup(x => x.GetByIdAsync(referral.Id))
                .ReturnsAsync(referral);

            // Act
            var exception = Assert.ThrowsAsync<UnauthorizedAccessException>(
                async () => await _classUnderTest.ExecuteAsync(referral.Id, "a.broker@hackney.gov.uk"));

            // Assert
            Assert.That(exception.Message, Is.EqualTo("Referral is not assigned to a.broker@hackney.gov.uk"));
        }
    }
}
