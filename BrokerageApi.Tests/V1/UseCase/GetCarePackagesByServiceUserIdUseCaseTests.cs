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
    public class GetCarePackagesByServiceUserIdUseCaseTests
    {
        private Mock<ICarePackageGateway> _mockCarePackageGateway;
        private GetCarePackagesByServiceUserIdUseCase _classUnderTest;
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _fixture = FixtureHelpers.Fixture;
            _mockCarePackageGateway = new Mock<ICarePackageGateway>();
            _classUnderTest = new GetCarePackagesByServiceUserIdUseCase(_mockCarePackageGateway.Object);
        }

        [Test]
        public async Task GetCarePackagesByServiceUserId()
        {
            // Arrange
            const string socialCareId = "aServiceUserId";
            var expectedCarePackages = _fixture.BuildCarePackage(socialCareId)
                .CreateMany();
            _mockCarePackageGateway
                .Setup(x => x.GetByServiceUserIdAsync("aServiceUserId"))
                .ReturnsAsync(expectedCarePackages);

            // Act
            var result = await _classUnderTest.ExecuteAsync("aServiceUserId");

            // Assert
            result.Should().BeEquivalentTo(expectedCarePackages);
        }

    }
}
