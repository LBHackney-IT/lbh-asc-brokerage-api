using System;
using System.Threading.Tasks;
using AutoFixture;
using BrokerageApi.Tests.V1.Helpers;
using BrokerageApi.V1.Boundary.Request;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.UseCase;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace BrokerageApi.Tests.V1.UseCase
{
    [TestFixture]
    public class ReassignBrokerToReferralUseCaseTests
    {
        private ReassignBrokerToReferralUseCase _classUnderTest;
        private Fixture _fixture;
        private Mock<IReferralGateway> _mockReferralGateway;
        private Mock<IDbSaver> _mockDbSaver;

        [SetUp]
        public void Setup()
        {
            _fixture = FixtureHelpers.Fixture;
            _mockReferralGateway = new Mock<IReferralGateway>();
            _mockDbSaver = new Mock<IDbSaver>();
            _classUnderTest = new ReassignBrokerToReferralUseCase(_mockReferralGateway.Object, _mockDbSaver.Object);
        }

        [Test]
        public async Task ReassignsBrokerToReferral()
        {
            // Arrange
            var request = _fixture.Build<AssignBrokerRequest>()
                .With(x => x.Broker, "a.broker@hackney.gov.uk")
                .Create();

            var referral = _fixture.Build<Referral>()
                .With(x => x.Status, ReferralStatus.Assigned)
                .With(x => x.AssignedTo, "other.broker@hackney.gov.uk")
                .Create();

            _mockReferralGateway
                .Setup(x => x.GetByIdAsync(referral.Id))
                .ReturnsAsync(referral);

            _mockDbSaver
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _classUnderTest.ExecuteAsync(referral.Id, request);

            // Assert
            Assert.That(result.Status, Is.EqualTo(ReferralStatus.Assigned));
            Assert.That(result.AssignedTo, Is.EqualTo("a.broker@hackney.gov.uk"));
            _mockDbSaver.Verify(x => x.SaveChangesAsync(), Times.Once());
        }

        [Test]
        public void ThrowsArgumentNullExceptionWhenReferralDoesntExist()
        {
            // Arrange
            var request = _fixture.Build<AssignBrokerRequest>()
                .With(x => x.Broker, "a.broker@hackney.gov.uk")
                .Create();

            _mockReferralGateway
                .Setup(x => x.GetByIdAsync(123456))
                .ReturnsAsync(null as Referral);

            // Act
            var exception = Assert.ThrowsAsync<ArgumentNullException>(
                async () => await _classUnderTest.ExecuteAsync(123456, request));

            // Assert
            Assert.That(exception.Message, Is.EqualTo("Referral not found for: 123456 (Parameter 'referralId')"));
        }

        [Test]
        public void ThrowsInvalidOperationExceptionWhenReferralIsUnassigned()
        {
            // Arrange
            var request = _fixture.Build<AssignBrokerRequest>()
                .With(x => x.Broker, "a.broker@hackney.gov.uk")
                .Create();

            var referral = _fixture.Build<Referral>()
                .With(x => x.Status, ReferralStatus.Unassigned)
                .Create();

            _mockReferralGateway
                .Setup(x => x.GetByIdAsync(referral.Id))
                .ReturnsAsync(referral);

            // Act
            var exception = Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _classUnderTest.ExecuteAsync(referral.Id, request));

            // Assert
            Assert.That(exception.Message, Is.EqualTo("Referral is not in a valid state for reassignment"));
        }
    }
}
