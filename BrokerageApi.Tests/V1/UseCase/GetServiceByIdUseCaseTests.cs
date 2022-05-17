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

        [Test]
        public void ThrowsArgumentNullExceptionWhenServiceDoesntExist()
        {
            // Arrange
            _mockServiceGateway
                .Setup(x => x.GetByIdAsync(123456))
                .ReturnsAsync(null as Service);

            // Act
            var exception = Assert.ThrowsAsync<ArgumentNullException>(
                async () => await _classUnderTest.ExecuteAsync(123456));

            // Assert
            Assert.That(exception.Message, Is.EqualTo("Service not found for: 123456 (Parameter 'id')"));
        }
    }
}
