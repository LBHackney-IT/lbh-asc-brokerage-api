using AutoFixture;
using BrokerageApi.Tests.V1.Controllers.Mocks;
using BrokerageApi.Tests.V1.Helpers;
using BrokerageApi.V1.Boundary.Request;
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

namespace BrokerageApi.Tests.V1.Controllers
{
    [TestFixture]
    public class ServicesControllerTests : ControllerTests
    {
        private Fixture _fixture;
        private Mock<IGetAllServicesUseCase> _getAllServicesUseCaseMock;
        private Mock<IGetServiceByIdUseCase> _getServiceByIdUseCaseMock;
        private MockProblemDetailsFactory _problemDetailsFactoryMock;

        private ServicesController _classUnderTest;

        [SetUp]
        public void SetUp()
        {
            _fixture = FixtureHelpers.Fixture;
            _getAllServicesUseCaseMock = new Mock<IGetAllServicesUseCase>();
            _getServiceByIdUseCaseMock = new Mock<IGetServiceByIdUseCase>();
            _problemDetailsFactoryMock = new MockProblemDetailsFactory();

            _classUnderTest = new ServicesController(
                _getAllServicesUseCaseMock.Object,
                _getServiceByIdUseCaseMock.Object
            );

            // .NET 3.1 doesn't set ProblemDetailsFactory so we need to mock it
            _classUnderTest.ProblemDetailsFactory = _problemDetailsFactoryMock.Object;
        }

        [Test]
        public async Task GetAllServices()
        {
            // Arrange
            var services = _fixture.CreateMany<Service>();
            _getAllServicesUseCaseMock.Setup(x => x.ExecuteAsync())
                .ReturnsAsync(services);

            // Act
            var objectResult = await _classUnderTest.GetAllServices();
            var statusCode = GetStatusCode(objectResult);
            var result = GetResultData<List<ServiceResponse>>(objectResult);

            // Assert
            statusCode.Should().Be((int) HttpStatusCode.OK);
            result.Should().BeEquivalentTo(services.Select(s => s.ToResponse()).ToList());
        }

        [Test]
        public async Task GetService()
        {
            // Arrange
            var service = _fixture.Create<Service>();
            _getServiceByIdUseCaseMock.Setup(x => x.ExecuteAsync(service.Id))
                .ReturnsAsync(service);

            // Act
            var objectResult = await _classUnderTest.GetService(service.Id);
            var statusCode = GetStatusCode(objectResult);
            var result = GetResultData<ServiceResponse>(objectResult);

            // Assert
            statusCode.Should().Be((int) HttpStatusCode.OK);
            result.Should().BeEquivalentTo(service.ToResponse());
        }
    }
}
