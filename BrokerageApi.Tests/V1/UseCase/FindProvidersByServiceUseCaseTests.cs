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
    public class FindProvidersByServiceUseCaseTests
    {
        private Mock<IProviderGateway> _mockProviderGateway;
        private FindProvidersByServiceUseCase _classUnderTest;
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _fixture = FixtureHelpers.Fixture;
            _mockProviderGateway = new Mock<IProviderGateway>();
            _classUnderTest = new FindProvidersByServiceUseCase(_mockProviderGateway.Object);
        }

        [Test]
        public async Task FindProvidersByService()
        {
            // Arrange
            var service = _fixture.Create<Service>();
            var expectedProviders = _fixture.CreateMany<Provider>();
            _mockProviderGateway
                .Setup(x => x.FindByServiceAsync(service, "Acme"))
                .ReturnsAsync(expectedProviders);

            // Act
            var result = await _classUnderTest.ExecuteAsync(service, "Acme");

            // Assert
            result.Should().BeEquivalentTo(expectedProviders);
        }
    }
}
