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
    public class GetServiceByIdUseCaseTests
    {
        private Mock<IServiceGateway> _mockServiceGateway;
        private GetServiceByIdUseCase _classUnderTest;
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _fixture = FixtureHelpers.Fixture;
            _mockServiceGateway = new Mock<IServiceGateway>();
            _classUnderTest = new GetServiceByIdUseCase(_mockServiceGateway.Object);
        }

        [Test]
        public async Task GetService()
        {
            // Arrange
            var service = _fixture.Create<Service>();
            _mockServiceGateway
                .Setup(x => x.GetByIdAsync(service.Id))
                .ReturnsAsync(service);

            // Act
            var result = await _classUnderTest.ExecuteAsync(service.Id);

            // Assert
            result.Should().BeEquivalentTo(service);
        }
    }
}
