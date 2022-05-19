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

    public class GetCarePackageByIdUseCaseTests
    {
        private Mock<ICarePackageGateway> _mockCarePackageGateway;
        private GetCarePackageByIdUseCase _classUnderTest;
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _fixture = FixtureHelpers.Fixture;
            _mockCarePackageGateway = new Mock<ICarePackageGateway>();
            _classUnderTest = new GetCarePackageByIdUseCase(_mockCarePackageGateway.Object);
        }

        [Test]
        public async Task GetCarePackage()
        {
            // Arrange
            var carePackage = _fixture.Create<CarePackage>();

            _mockCarePackageGateway
                .Setup(x => x.GetByIdAsync(carePackage.Id))
                .ReturnsAsync(carePackage);

            // Act
            var result = await _classUnderTest.ExecuteAsync(carePackage.Id);

            // Assert
            result.Should().BeEquivalentTo(carePackage);
        }

        [Test]
        public void ThrowsArgumentNullExceptionWhenCarePackageDoesntExist()
        {
            // Arrange
            _mockCarePackageGateway
                .Setup(x => x.GetByIdAsync(123456))
                .ReturnsAsync(null as CarePackage);

            // Act
            var exception = Assert.ThrowsAsync<ArgumentNullException>(
                async () => await _classUnderTest.ExecuteAsync(123456));

            // Assert
            Assert.That(exception.Message, Is.EqualTo("Care package not found for: 123456 (Parameter 'id')"));
        }
    }
}
