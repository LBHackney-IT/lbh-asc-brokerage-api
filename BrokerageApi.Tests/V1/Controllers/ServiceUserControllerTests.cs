using AutoFixture;
using BrokerageApi.Tests.V1.Controllers.Mocks;
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
        public async Task GetCarePackagesByServiceUserId()
        {
            // Arrange
            const string socialCareId = "expectedId";
            var carePackages = _fixture.BuildCarePackage(socialCareId)
                .CreateMany();

            _mockGetCarePackagesByServiceUserIdUseCase
                .Setup(x => x.ExecuteAsync(socialCareId))
                .ReturnsAsync(carePackages);
            // Act
            var objectResult = await _classUnderTest.GetServiceUserCarePackages(socialCareId);
            var statusCode = GetStatusCode(objectResult);
            var result = GetResultData<List<CarePackageResponse>>(objectResult);

            // Assert
            statusCode.Should().Be((int) HttpStatusCode.OK);
            result.Should().BeEquivalentTo(carePackages.Select(r => r.ToResponse()).ToList());
        }

        [Test]
        public async Task GetNoCarePackagesWhenServiceUserDoesNotExist()
        {
            // Arrange
            _mockGetCarePackagesByServiceUserIdUseCase
                .Setup(x => x.ExecuteAsync("unexpectedId"))
                .Callback((String socialCareId) => throw new ArgumentNullException(nameof(socialCareId), "No care packages found for this service user"))
                .Returns(Task.FromResult(new List<CarePackage>() as IEnumerable<CarePackage>));


            // Act
            var objectResult = await _classUnderTest.GetServiceUserCarePackages("unexpectedId");
            var statusCode = GetStatusCode(objectResult);

            // Assert
            statusCode.Should().Be((int) HttpStatusCode.NotFound);
            _mockProblemDetailsFactory.VerifyStatusCode(HttpStatusCode.NotFound);
        }


    }
}
