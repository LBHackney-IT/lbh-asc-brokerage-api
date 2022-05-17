using AutoFixture;
using System;
using System.Threading.Tasks;
using BrokerageApi.Tests.V1.Helpers;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.UseCase;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace BrokerageApi.Tests.V1.UseCase
{
    public class GetReferralByIdUseCaseTests
    {
        private Mock<IReferralGateway> _mockReferralGateway;
        private GetReferralByIdUseCase _classUnderTest;
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _fixture = FixtureHelpers.Fixture;
            _mockReferralGateway = new Mock<IReferralGateway>();
            _classUnderTest = new GetReferralByIdUseCase(_mockReferralGateway.Object);
        }

        [Test]
        public async Task GetReferral()
        {
            // Arrange
            var referral = _fixture.Create<Referral>();

            _mockReferralGateway
                .Setup(x => x.GetByIdAsync(referral.Id))
                .ReturnsAsync(referral);

            // Act
            var result = await _classUnderTest.ExecuteAsync(referral.Id);

            // Assert
            result.Should().BeEquivalentTo(referral);
        }

        [Test]
        public void ThrowsArgumentNullExceptionWhenReferralDoesntExist()
        {
            // Arrange
            _mockReferralGateway
                .Setup(x => x.GetByIdAsync(123456))
                .ReturnsAsync(null as Referral);

            // Act
            var exception = Assert.ThrowsAsync<ArgumentNullException>(
                async () => await _classUnderTest.ExecuteAsync(123456));

            // Assert
            Assert.That(exception.Message, Is.EqualTo("Referral not found for: 123456 (Parameter 'id')"));
        }
    }
}
