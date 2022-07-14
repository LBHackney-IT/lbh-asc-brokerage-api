using AutoFixture;
using System.Threading.Tasks;
using BrokerageApi.Tests.V1.Helpers;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.UseCase;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace BrokerageApi.Tests.V1.UseCase
{
    public class GetServiceUsersByRequestUseCaseTests
    {
        private Mock<IServiceUserGateway> _mockServiceUserGateway;
        private Mock<IElementGateway> _mockElementGateway;

        private GetServiceUserByRequestUseCase _classUnderTest;
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _fixture = FixtureHelpers.Fixture;
            _mockServiceUserGateway = new Mock<IServiceUserGateway>();
            _mockElementGateway = new Mock<IElementGateway>();

            _classUnderTest = new GetServiceUserByRequestUseCase(_mockServiceUserGateway.Object, _mockElementGateway.Object);
        }

        [Test]
        public async Task CanGetServiceUsers()
        {
            // Arrange
            var expectedServiceUsers = _fixture.BuildServiceUser().CreateMany();
            var serviceUserRequest = _fixture.BuildServiceUserRequest("12345").Create();
            _mockServiceUserGateway
                .Setup(x => x.GetByRequestAsync(serviceUserRequest))
                .ReturnsAsync(expectedServiceUsers);

            // Act
            var result = await _classUnderTest.ExecuteAsync(serviceUserRequest);

            // Assert
            result.Should().BeEquivalentTo(expectedServiceUsers);
        }
    }
}
