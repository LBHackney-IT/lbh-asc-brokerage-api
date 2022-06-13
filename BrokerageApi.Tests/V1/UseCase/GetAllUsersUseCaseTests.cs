using AutoFixture;
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
    public class GetAllUsersUseCaseTests
    {
        private Mock<IUserGateway> _mockUserGateway;
        private GetAllUsersUseCase _classUnderTest;
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _fixture = FixtureHelpers.Fixture;
            _mockUserGateway = new Mock<IUserGateway>();
            _classUnderTest = new GetAllUsersUseCase(_mockUserGateway.Object);
        }

        [Test]
        public async Task GetAllUsers()
        {
            // Arrange
            var expectedUsers = _fixture.BuildUser().CreateMany();
            _mockUserGateway
                .Setup(x => x.GetAllAsync(null))
                .ReturnsAsync(expectedUsers);

            // Act
            var result = await _classUnderTest.ExecuteAsync(null);

            // Assert
            result.Should().BeEquivalentTo(expectedUsers);
        }

        [Test]
        public async Task GetFilteredAllUsers()
        {
            // Arrange
            var expectedUsers = _fixture.BuildUser().CreateMany();
            _mockUserGateway
                .Setup(x => x.GetAllAsync(UserRole.BrokerageAssistant))
                .ReturnsAsync(expectedUsers);

            // Act
            var result = await _classUnderTest.ExecuteAsync(UserRole.BrokerageAssistant);

            // Assert
            result.Should().BeEquivalentTo(expectedUsers);
        }
    }
}
