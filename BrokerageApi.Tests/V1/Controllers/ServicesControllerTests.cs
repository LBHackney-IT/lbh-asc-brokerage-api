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
    public class ServicesControllerTests : ControllerTests
    {
        private Fixture _fixture;
        private Mock<IGetAllServicesUseCase> _mockGetAllServicesUseCase;
        private Mock<IGetServiceByIdUseCase> _mockGetServiceByIdUseCase;
        private Mock<IFindProvidersUseCase> _mockFindProvidersUseCase;

        private ServicesController _classUnderTest;

        [SetUp]
        public void SetUp()
        {
            _fixture = FixtureHelpers.Fixture;
            _mockGetAllServicesUseCase = new Mock<IGetAllServicesUseCase>();
            _mockGetServiceByIdUseCase = new Mock<IGetServiceByIdUseCase>();
            _mockFindProvidersUseCase = new Mock<IFindProvidersUseCase>();

            _classUnderTest = new ServicesController(
                _mockGetAllServicesUseCase.Object,
                _mockGetServiceByIdUseCase.Object,
                _mockFindProvidersUseCase.Object
            );
        }

        [Test]
        public async Task GetAllServices()
        {
            // Arrange
            var services = _fixture.BuildService().CreateMany();
            _mockGetAllServicesUseCase
                .Setup(x => x.ExecuteAsync())
                .ReturnsAsync(services);

            // Act
            var response = await _classUnderTest.GetAllServices();
            var statusCode = GetStatusCode(response);
            var result = GetResultData<List<ServiceResponse>>(response);

            // Assert
            statusCode.Should().Be((int) HttpStatusCode.OK);
            result.Should().BeEquivalentTo(services.Select(s => s.ToResponse()).ToList());
        }

        [Test]
        public async Task GetService()
        {
            // Arrange
            var service = _fixture.BuildService().Create();
            _mockGetServiceByIdUseCase
                .Setup(x => x.ExecuteAsync(service.Id))
                .ReturnsAsync(service);

            // Act
            var response = await _classUnderTest.GetService(service.Id);
            var statusCode = GetStatusCode(response);
            var result = GetResultData<ServiceResponse>(response);

            // Assert
            statusCode.Should().Be((int) HttpStatusCode.OK);
            result.Should().BeEquivalentTo(service.ToResponse());
        }

        [Test]
        public async Task GetServiceWhenDoesNotExist()
        {
            // Arrange
            _mockGetServiceByIdUseCase
                .Setup(x => x.ExecuteAsync(123456))
                .Callback((int id) => throw new ArgumentNullException(nameof(id), "Service not found for: 123456"))
                .Returns(Task.FromResult(new Service()));

            // Act
            var response = await _classUnderTest.GetService(123456);
            var statusCode = GetStatusCode(response);
            var result = GetResultData<ProblemDetails>(response);

            // Assert
            statusCode.Should().Be((int) HttpStatusCode.NotFound);
            result.Status.Should().Be((int) HttpStatusCode.NotFound);
            result.Detail.Should().Be("Service not found for: 123456 (Parameter 'id')");
        }
    }
}
