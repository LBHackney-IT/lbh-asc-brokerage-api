using AutoFixture;
using System;
using System.Threading.Tasks;
using BrokerageApi.Tests.V1.Helpers;
using BrokerageApi.V1.Gateways;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.UseCase;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace BrokerageApi.Tests.V1.UseCase
{
    public class GetCurrentReferralsUseCaseTests
    {
        private Mock<IReferralGateway> _mockReferralGateway;
        private GetCurrentReferralsUseCase _classUnderTest;
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _fixture = FixtureHelpers.Fixture;
            _mockReferralGateway = new Mock<IReferralGateway>();
            _classUnderTest = new GetCurrentReferralsUseCase(_mockReferralGateway.Object);
        }

        [Test]
        public async Task GetCurrentReferrals()
        {
            // Arrange
            var expectedReferrals = _fixture.CreateMany<Referral>();
            _mockReferralGateway
                .Setup(x => x.GetCurrentAsync(null))
                .ReturnsAsync(expectedReferrals);

            // Act
            var result = await _classUnderTest.ExecuteAsync(null);

            // Assert
            result.Should().BeEquivalentTo(expectedReferrals);
        }

        [Test]
        public async Task GetFilteredCurrentReferrals()
        {
            // Arrange
            var expectedReferrals = _fixture.CreateMany<Referral>();
            _mockReferralGateway
                .Setup(x => x.GetCurrentAsync(ReferralStatus.Unassigned))
                .ReturnsAsync(expectedReferrals);

            // Act
            var result = await _classUnderTest.ExecuteAsync(ReferralStatus.Unassigned);

            // Assert
            result.Should().BeEquivalentTo(expectedReferrals);
        }
    }
}
