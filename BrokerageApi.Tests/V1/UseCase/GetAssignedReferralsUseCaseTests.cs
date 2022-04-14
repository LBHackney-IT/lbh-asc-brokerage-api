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
    public class GetAssignedReferralsUseCaseTests
    {
        private Mock<IReferralGateway> _mockReferralGateway;
        private GetAssignedReferralsUseCase _classUnderTest;
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _fixture = FixtureHelpers.Fixture;
            _mockReferralGateway = new Mock<IReferralGateway>();
            _classUnderTest = new GetAssignedReferralsUseCase(_mockReferralGateway.Object);
        }

        [Test]
        public async Task GetAssignedReferrals()
        {
            // Arrange
            var expectedReferrals = _fixture.CreateMany<Referral>();
            _mockReferralGateway
                .Setup(x => x.GetAssignedAsync("a.broker@hackney.gov.uk", null))
                .ReturnsAsync(expectedReferrals);

            // Act
            var result = await _classUnderTest.ExecuteAsync("a.broker@hackney.gov.uk", null);

            // Assert
            result.Should().BeEquivalentTo(expectedReferrals);
        }

        [Test]
        public async Task GetFilteredAssignedReferrals()
        {
            // Arrange
            var expectedReferrals = _fixture.CreateMany<Referral>();
            _mockReferralGateway
                .Setup(x => x.GetAssignedAsync("a.broker@hackney.gov.uk", ReferralStatus.Unassigned))
                .ReturnsAsync(expectedReferrals);

            // Act
            var result = await _classUnderTest.ExecuteAsync("a.broker@hackney.gov.uk", ReferralStatus.Unassigned);

            // Assert
            result.Should().BeEquivalentTo(expectedReferrals);
        }
    }
}
