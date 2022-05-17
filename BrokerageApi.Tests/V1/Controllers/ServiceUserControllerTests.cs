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
using Microsoft.AspNetCore.Http;

namespace BrokerageApi.Tests.V1.Controllers
{
    [TestFixture]
    public class ServiceUserControllerTests : ControllerTests
    {
        private Fixture _fixture;
        private Mock<IGetCarePackagesByServiceUserIdUseCase> _mockGetCarePackagesByServiceUserIdUseCase;
        private MockProblemDetailsFactory _mockProblemDetailsFactory;


        private ServiceUserController _classUnderTest;

        [SetUp]
        public void SetUp()
        {
            _fixture = FixtureHelpers.Fixture;
            _mockGetCarePackagesByServiceUserIdUseCase = new Mock<IGetCarePackagesByServiceUserIdUseCase>();
            _mockProblemDetailsFactory = new MockProblemDetailsFactory();


            _classUnderTest = new ServiceUserController(
                _mockGetCarePackagesByServiceUserIdUseCase.Object
            );

            // .NET 3.1 doesn't set ProblemDetailsFactory so we need to mock it
            _classUnderTest.ProblemDetailsFactory = _mockProblemDetailsFactory.Object;

            SetupAuthentication(_classUnderTest);
        }
        //this test is not working
        [Test]
        public async Task GetCarePackages()
        {
            // Arrange
            var carePackages = _fixture.CreateMany<CarePackage>();
            _mockGetCarePackagesByServiceUserIdUseCase
                .Setup(x => x.ExecuteAsync(null))
                .ReturnsAsync(carePackages);
            var serviceUserId = carePackages.ElementAt(0).SocialCareId;
            // Act
            var objectResult = await _classUnderTest.GetServiceUserCarePackages(serviceUserId);
            var statusCode = GetStatusCode(objectResult);
            var result = GetResultData<List<CarePackageResponse>>(objectResult);

            // Assert
            statusCode.Should().Be((int) HttpStatusCode.OK);
            result.Should().BeEquivalentTo(carePackages.Select(r => r.ToResponse()).ToList());
        }

        // [Test]
        // public async Task GetCarePackageWhenDoesNotExist()
        // {
        //     // Arrange
        //      _mockGetCarePackagesByServiceUserIdUseCase
        //         .Setup(x => x.ExecuteAsync(0000))
        //         .Callback((int id) => throw new ArgumentNullException(nameof(id), "Care package not found for: 00000"))
        //         .Returns(Task.FromResult(new CarePackage()));

        //     // Act
        //     var objectResult = await _classUnderTest.GetServiceUserCarePackages(serviceUserId);
        //     var statusCode = GetStatusCode(objectResult);

        //     // Assert
        //     statusCode.Should().Be((int) HttpStatusCode.NotFound);
        //     _mockProblemDetailsFactory.VerifyStatusCode(HttpStatusCode.NotFound);
        // }


    }
}
