using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
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
using NodaTime;

namespace BrokerageApi.Tests.V1.Controllers
{
    public class ServiceUserControllerTests : ControllerTests
    {
        private Mock<IGetServiceOverviewUseCase> _mockGetServiceOverviewUseCase;
        private ServiceUserController _classUnderTest;
        private Fixture _fixture;
        private MockProblemDetailsFactory _mockProblemDetailsFactory;
        private Mock<IGetCarePackagesByServiceUserIdUseCase> _mockGetCarePackagesByServiceUserIdUseCase;

        private Mock<IGetServiceUserByRequestUseCase> _mockGetServiceUserByRequestUseCase;

        [SetUp]
        public void SetUp()
        {
            _fixture = FixtureHelpers.Fixture;
            _mockGetServiceOverviewUseCase = new Mock<IGetServiceOverviewUseCase>();
            _mockGetCarePackagesByServiceUserIdUseCase = new Mock<IGetCarePackagesByServiceUserIdUseCase>();
            _mockGetServiceUserByRequestUseCase = new Mock<IGetServiceUserByRequestUseCase>();
            _mockProblemDetailsFactory = new MockProblemDetailsFactory();

            _classUnderTest = new ServiceUserController(
                _mockGetServiceOverviewUseCase.Object,
                _mockGetCarePackagesByServiceUserIdUseCase.Object,
                _mockGetServiceUserByRequestUseCase.Object
                );
            _classUnderTest.ProblemDetailsFactory = _mockProblemDetailsFactory.Object;
            SetupAuthentication(_classUnderTest);

        }

        [Test]
        public async Task CanGetServiceOverview()
        {
            const string socialCareId = "expectedId";
            var elements = _fixture.BuildElement(1, 1)
                .With(e => e.SocialCareId, socialCareId)
                .CreateMany();
            _mockGetServiceOverviewUseCase.Setup(x => x.ExecuteAsync(socialCareId))
                .ReturnsAsync(elements);

            var objectResult = await _classUnderTest.GetServiceOverview(socialCareId);
            var statusCode = GetStatusCode(objectResult);
            var result = GetResultData<List<ElementResponse>>(objectResult);

            statusCode.Should().Be((int) HttpStatusCode.OK);
            result.Should().BeEquivalentTo(elements.Select(e => e.ToResponse()));
        }

        [Test]
        public async Task Returns404WhenOverviewNotFound()
        {
            const string socialCareId = "expectedId";
            _mockGetServiceOverviewUseCase.Setup(x => x.ExecuteAsync(socialCareId))
                .ThrowsAsync(new ArgumentException("test"));

            var objectResult = await _classUnderTest.GetServiceOverview(socialCareId);
            var statusCode = GetStatusCode(objectResult);

            statusCode.Should().Be((int) HttpStatusCode.NotFound);
            _mockProblemDetailsFactory.VerifyProblem(HttpStatusCode.NotFound);
        }
        [Test]
        public async Task CanGetCarePackagesByServiceUserId()
        {
            // Arrange
            const string socialCareId = "expectedId";
            var elements = _fixture.BuildElement(1, 1)
                          .CreateMany();
            var carePackages = _fixture.BuildCarePackage(socialCareId)
                .With(c => c.Elements, elements.ToList)
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
            _mockProblemDetailsFactory.VerifyProblem(HttpStatusCode.NotFound);
        }
        [Test]
        public async Task CanGetServiceUserByRequest()
        {
            //Arrange
            var serviceUsers = _fixture.BuildServiceUser()
            .CreateMany();

            var serviceUserRequest = _fixture.BuildServiceUserRequest(serviceUsers.ElementAt(0).SocialCareId)
            .Create();

            _mockGetServiceUserByRequestUseCase
                .Setup(x => x.ExecuteAsync(serviceUserRequest))
                .ReturnsAsync(serviceUsers);
            //Act
            var objectResult = await _classUnderTest.GetServiceUser(serviceUserRequest);
            var statusCode = GetStatusCode(objectResult);
            var result = GetResultData<List<ServiceUserResponse>>(objectResult);

            //Assert
            statusCode.Should().Be((int) HttpStatusCode.OK);
            result.Should().BeEquivalentTo(serviceUsers.Select(su => su.ToResponse()));

        }

        [Test]
        public async Task GetsExceptionWhenBadRequest()
        {
            //Arrange
            var serviceUsers = _fixture.BuildServiceUser()
            .CreateMany();

            var serviceUserRequest = _fixture.BuildServiceUserRequest("notThatServiceUser")
            .Create();

            _mockGetServiceUserByRequestUseCase
                .Setup(x => x.ExecuteAsync(serviceUserRequest))
                .ThrowsAsync(new ArgumentException("Nope"));
            //Act
            var objectResult = await _classUnderTest.GetServiceUser(serviceUserRequest);
            var statusCode = GetStatusCode(objectResult);

            //Assert
            statusCode.Should().Be((int) HttpStatusCode.BadRequest);
            _mockProblemDetailsFactory.VerifyProblem(HttpStatusCode.BadRequest);

        }        
    }
}
