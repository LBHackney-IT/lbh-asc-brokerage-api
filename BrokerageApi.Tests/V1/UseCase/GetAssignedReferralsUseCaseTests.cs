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

    public class GetAssignedReferralsUseCaseTests
    {
        private Mock<IReferralGateway> _mockReferralGateway;
        private Mock<IUserService> _mockUserService;
        private GetAssignedReferralsUseCase _classUnderTest;
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _fixture = FixtureHelpers.Fixture;
            _mockReferralGateway = new Mock<IReferralGateway>();
            _mockUserService = new Mock<IUserService>();

            _classUnderTest = new GetAssignedReferralsUseCase(
                _mockReferralGateway.Object,
                _mockUserService.Object
            );
        }

        [Test]
        public async Task GetAssignedReferrals()
        {
            // Arrange
            var expectedReferrals = _fixture.BuildReferral(ReferralStatus.Assigned).CreateMany();

            _mockUserService
                .SetupGet(x => x.Email)
                .Returns("a.broker@hackney.gov.uk");

            _mockReferralGateway
                .Setup(x => x.GetAssignedAsync("a.broker@hackney.gov.uk", null))
                .ReturnsAsync(expectedReferrals);

            // Act
            var result = await _classUnderTest.ExecuteAsync(null);

            // Assert
            result.Should().BeEquivalentTo(expectedReferrals);
        }

        [Test]
        public async Task GetFilteredAssignedReferrals()
        {
            // Arrange
            var expectedReferrals = _fixture.BuildReferral(ReferralStatus.Assigned).CreateMany();

            _mockUserService
                .SetupGet(x => x.Email)
                .Returns("a.broker@hackney.gov.uk");

            _mockReferralGateway
                .Setup(x => x.GetAssignedAsync("a.broker@hackney.gov.uk", ReferralStatus.Unassigned))
                .ReturnsAsync(expectedReferrals);

            // Act
            var result = await _classUnderTest.ExecuteAsync(ReferralStatus.Unassigned);

            // Assert
            result.Should().BeEquivalentTo(expectedReferrals);
        }
    }
}
