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
    public class FindProvidersByServiceIdUseCaseTests
    {
        private Mock<IServiceGateway> _mockServiceGateway;
        private Mock<IProviderGateway> _mockProviderGateway;
        private FindProvidersByServiceIdUseCase _classUnderTest;
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _fixture = FixtureHelpers.Fixture;
            _mockServiceGateway = new Mock<IServiceGateway>();
            _mockProviderGateway = new Mock<IProviderGateway>();
            _classUnderTest = new FindProvidersByServiceIdUseCase(
                _mockServiceGateway.Object,
                _mockProviderGateway.Object
            );
        }

        [Test]
        public async Task FindProvidersByService()
        {
            // Arrange
            var service = _fixture.Create<Service>();
            var expectedProviders = _fixture.CreateMany<Provider>();

            _mockServiceGateway
                .Setup(x => x.GetByIdAsync(service.Id))
                .ReturnsAsync(service);

            _mockProviderGateway
                .Setup(x => x.FindByServiceIdAsync(service.Id, "Acme"))
                .ReturnsAsync(expectedProviders);

            // Act
            var result = await _classUnderTest.ExecuteAsync(service.Id, "Acme");

            // Assert
            result.Should().BeEquivalentTo(expectedProviders);
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
                async () => await _classUnderTest.ExecuteAsync(123456, "Acme"));

            // Assert
            Assert.That(exception.Message, Is.EqualTo("Service not found for: 123456 (Parameter 'serviceId')"));
        }
    }
}
