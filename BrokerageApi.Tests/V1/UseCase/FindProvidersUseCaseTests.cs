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
    public class FindProvidersUseCaseTests
    {
        private Mock<IProviderGateway> _mockProviderGateway;
        private FindProvidersUseCase _classUnderTest;
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _fixture = FixtureHelpers.Fixture;
            _mockProviderGateway = new Mock<IProviderGateway>();
            _classUnderTest = new FindProvidersUseCase(_mockProviderGateway.Object);
        }

        [Test]
        public async Task FindProviders()
        {
            // Arrange
            var expectedProviders = _fixture.BuildProvider().CreateMany();

            _mockProviderGateway
                .Setup(x => x.FindAsync("Acme"))
                .ReturnsAsync(expectedProviders);

            // Act
            var result = await _classUnderTest.ExecuteAsync("Acme");

            // Assert
            result.Should().BeEquivalentTo(expectedProviders);
        }
    }
}
