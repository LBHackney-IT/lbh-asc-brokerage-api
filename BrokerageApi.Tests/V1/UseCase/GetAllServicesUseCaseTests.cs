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
    public class GetAllServicesUseCaseTests
    {
        private Mock<IServiceGateway> _mockServiceGateway;
        private GetAllServicesUseCase _classUnderTest;
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _fixture = FixtureHelpers.Fixture;
            _mockServiceGateway = new Mock<IServiceGateway>();
            _classUnderTest = new GetAllServicesUseCase(_mockServiceGateway.Object);
        }

        [Test]
        public async Task GetAllServices()
        {
            // Arrange
            var expectedServices = _fixture.BuildService().CreateMany();
            _mockServiceGateway
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(expectedServices);

            // Act
            var result = await _classUnderTest.ExecuteAsync();

            // Assert
            result.Should().BeEquivalentTo(expectedServices);
        }

        [Test]
        public async Task GetFilteredAllServices()
        {
            // Arrange
            var expectedServices = _fixture.BuildService().CreateMany();
            _mockServiceGateway
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(expectedServices);

            // Act
            var result = await _classUnderTest.ExecuteAsync();

            // Assert
            result.Should().BeEquivalentTo(expectedServices);
        }
    }
}
