using AutoFixture;
using BrokerageApi.Tests.V1.Helpers;
using BrokerageApi.V1.Boundary.Response;
using BrokerageApi.V1.Controllers;
using BrokerageApi.V1.Factories;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.UseCase.Interfaces;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace BrokerageApi.Tests.V1.Controllers
{
    [TestFixture]
    public class ProvidersControllerTests : ControllerTests
    {
        private Fixture _fixture;
        private Mock<IFindProvidersUseCase> _mockFindProvidersUseCase;

        private ProvidersController _classUnderTest;

        [SetUp]
        public void SetUp()
        {
            _fixture = FixtureHelpers.Fixture;
            _mockFindProvidersUseCase = new Mock<IFindProvidersUseCase>();

            _classUnderTest = new ProvidersController(
                _mockFindProvidersUseCase.Object
            );
        }

        [Test]
        public async Task FindProviders()
        {
            // Arrange
            var providers = _fixture.BuildProvider().CreateMany();
            _mockFindProvidersUseCase
                .Setup(x => x.ExecuteAsync("Acme"))
                .ReturnsAsync(providers);

            // Act
            var response = await _classUnderTest.FindProviders("Acme");
            var statusCode = GetStatusCode(response);
            var result = GetResultData<List<ProviderResponse>>(response);

            // Assert
            statusCode.Should().Be((int) HttpStatusCode.OK);
            result.Should().BeEquivalentTo(providers.Select(s => s.ToResponse()).ToList());
        }
    }
}
