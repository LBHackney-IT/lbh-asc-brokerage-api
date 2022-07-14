using AutoFixture;
using System.Threading.Tasks;
using BrokerageApi.Tests.V1.Helpers;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.Services.Interfaces;
using BrokerageApi.V1.UseCase;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace BrokerageApi.Tests.V1.UseCase
{

    public class GetApprovedReferralsUseCaseTests
    {
        private Mock<IReferralGateway> _mockReferralGateway;
        private GetApprovedReferralsUseCase _classUnderTest;
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _fixture = FixtureHelpers.Fixture;
            _mockReferralGateway = new Mock<IReferralGateway>();

            _classUnderTest = new GetApprovedReferralsUseCase(
                _mockReferralGateway.Object
            );
        }

        [Test]
        public async Task GetApprovedReferrals()
        {
            // Arrange
            var expectedReferrals = _fixture.BuildReferral(ReferralStatus.Approved).CreateMany();

            _mockReferralGateway
                .Setup(x => x.GetApprovedAsync())
                .ReturnsAsync(expectedReferrals);

            // Act
            var result = await _classUnderTest.ExecuteAsync();

            // Assert
            result.Should().BeEquivalentTo(expectedReferrals);
        }
    }
}
