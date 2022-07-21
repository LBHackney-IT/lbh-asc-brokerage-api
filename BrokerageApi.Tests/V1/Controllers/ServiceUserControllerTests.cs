using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AutoFixture;
using BrokerageApi.Tests.V1.Helpers;
using BrokerageApi.V1.Boundary.Response;
using BrokerageApi.V1.Controllers;
using BrokerageApi.V1.Factories;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.UseCase.Interfaces;
using BrokerageApi.V1.UseCase.Interfaces.ServiceUsers;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace BrokerageApi.Tests.V1.Controllers
{
    public class ServiceUserControllerTests : ControllerTests
    {
        private Mock<IGetServiceOverviewsUseCase> _mockGetServiceOverviewsUseCase;
        private Mock<IGetServiceOverviewByIdUseCase> _mockGetServiceOverviewByIdUseCase;
        private Mock<IGetCarePackagesByServiceUserIdUseCase> _mockGetCarePackagesByServiceUserIdUseCase;
        private Mock<IGetServiceUserByRequestUseCase> _mockGetServiceUserByRequestUseCase;
        private Fixture _fixture;
        private ServiceUserController _classUnderTest;

        private Mock<IEditServiceUserUseCase> _mockEditServiceUserUsecase;


        [SetUp]
        public void SetUp()
        {
            _fixture = FixtureHelpers.Fixture;
            _mockGetServiceOverviewsUseCase = new Mock<IGetServiceOverviewsUseCase>();
            _mockGetServiceOverviewByIdUseCase = new Mock<IGetServiceOverviewByIdUseCase>();
            _mockGetCarePackagesByServiceUserIdUseCase = new Mock<IGetCarePackagesByServiceUserIdUseCase>();
            _mockGetServiceUserByRequestUseCase = new Mock<IGetServiceUserByRequestUseCase>();
            _mockEditServiceUserUsecase = new Mock<IEditServiceUserUseCase>();


            _classUnderTest = new ServiceUserController(
                _mockGetServiceOverviewsUseCase.Object,
                _mockGetServiceOverviewByIdUseCase.Object,
                _mockGetCarePackagesByServiceUserIdUseCase.Object,
                _mockGetServiceUserByRequestUseCase.Object,
                _mockEditServiceUserUsecase.Object
                );

            SetupAuthentication(_classUnderTest);
        }

        [Test]
        public async Task CanGetServiceOverviews()
        {
            // Arrange
            const string socialCareId = "expectedId";
            var serviceOverviews = _fixture.BuildServiceOverview().CreateMany();

            _mockGetServiceOverviewsUseCase
                .Setup(x => x.ExecuteAsync(socialCareId))
                .ReturnsAsync(serviceOverviews);

            // Act
            var response = await _classUnderTest.GetServiceOverviews(socialCareId);
            var statusCode = GetStatusCode(response);
            var result = GetResultData<List<ServiceOverviewResponse>>(response);

            // Assert
            statusCode.Should().Be((int) HttpStatusCode.OK);
            result.Should().BeEquivalentTo(serviceOverviews.Select(e => e.ToResponse()));
        }

        [Test]
        public async Task Returns404WhenServiceUserNotFound()
        {
            // Arrange
            const string socialCareId = "expectedId";

            _mockGetServiceOverviewsUseCase
                .Setup(x => x.ExecuteAsync(socialCareId))
                .Callback((string socialCareId) => throw new ArgumentNullException(nameof(socialCareId), "Service user not found for: expectedId"))
                .Returns(Task.FromResult(new List<ServiceOverview>() as IEnumerable<ServiceOverview>));

            // Act
            var response = await _classUnderTest.GetServiceOverviews(socialCareId);
            var statusCode = GetStatusCode(response);
            var result = GetResultData<ProblemDetails>(response);

            // Assert
            statusCode.Should().Be((int) HttpStatusCode.NotFound);
            result.Status.Should().Be((int) HttpStatusCode.NotFound);
            result.Detail.Should().Be("Service user not found for: expectedId (Parameter 'socialCareId')");
        }

        [Test]
        public async Task CanGetServiceOverviewById()
        {
            // Arrange
            const string socialCareId = "expectedId";
            const int serviceId = 1;

            var elements = _fixture.BuildServiceOverviewElement().CreateMany().ToList();
            var serviceOverview = _fixture.BuildServiceOverview().With(so => so.Elements, elements).Create();

            _mockGetServiceOverviewByIdUseCase
                .Setup(x => x.ExecuteAsync(socialCareId, serviceId))
                .ReturnsAsync(serviceOverview);

            // Act
            var response = await _classUnderTest.GetServiceOverviewById(socialCareId, serviceId);
            var statusCode = GetStatusCode(response);
            var result = GetResultData<ServiceOverviewResponse>(response);

            statusCode.Should().Be((int) HttpStatusCode.OK);
            result.Should().BeEquivalentTo(serviceOverview.ToResponse());
        }

        [Test]
        public async Task Returns404WhenServiceNotFound()
        {
            // Arrange
            const string socialCareId = "expectedId";
            const int serviceId = 1;

            _mockGetServiceOverviewByIdUseCase
                .Setup(x => x.ExecuteAsync(socialCareId, serviceId))
                .Callback((string socialCareId, int serviceId) => throw new ArgumentNullException(nameof(serviceId), "Service not found for: 1"))
                .Returns(Task.FromResult(null as ServiceOverview));

            // Act
            var response = await _classUnderTest.GetServiceOverviewById(socialCareId, serviceId);
            var statusCode = GetStatusCode(response);
            var result = GetResultData<ProblemDetails>(response);

            // Assert
            statusCode.Should().Be((int) HttpStatusCode.NotFound);
            result.Status.Should().Be((int) HttpStatusCode.NotFound);
            result.Detail.Should().Be("Service not found for: 1 (Parameter 'serviceId')");
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
            var response = await _classUnderTest.GetServiceUserCarePackages(socialCareId);
            var statusCode = GetStatusCode(response);
            var result = GetResultData<List<CarePackageResponse>>(response);

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
            var response = await _classUnderTest.GetServiceUserCarePackages("unexpectedId");
            var statusCode = GetStatusCode(response);
            var result = GetResultData<ProblemDetails>(response);

            // Assert
            statusCode.Should().Be((int) HttpStatusCode.NotFound);
            result.Status.Should().Be((int) HttpStatusCode.NotFound);
            result.Detail.Should().Be("No care packages found for this service user (Parameter 'socialCareId')");
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
        }
        [Test]
        public async Task CanUpdateCedarNumberOnServiceUser()
        {
            //Arrange
            var serviceUsers = _fixture.BuildServiceUser()
            .CreateMany();
            var aServiceUser = serviceUsers.ElementAt(0);
            var serviceUserRequest = _fixture.BuildEditServiceUserRequest(aServiceUser.SocialCareId)
            .Create();

            _mockEditServiceUserUsecase
                .Setup(x => x.ExecuteAsync(serviceUserRequest))
                .ReturnsAsync(aServiceUser);
            var objectResult = await _classUnderTest.UpdateServiceUserCedarNumber(serviceUserRequest);
            var statusCode = GetStatusCode(objectResult);
            var result = GetResultData<ServiceUserResponse>(objectResult);

            //Assert
            statusCode.Should().Be((int) HttpStatusCode.OK);
            result.Should().BeEquivalentTo(aServiceUser.ToResponse());
        }

        [TestCaseSource(nameof(_editServiceUserErrorList)), Property("AsUser", "CareChargeOfficer")]
        public async Task UpdateCedarNumberGetsExceptionWhenBadRequest(Exception exception, HttpStatusCode expectedStatusCode)
        {
            //Arrange
            var serviceUsers = _fixture.BuildServiceUser()
            .CreateMany();

            var serviceUserRequest = _fixture.BuildEditServiceUserRequest("notThatServiceUser")
            .Create();

            _mockEditServiceUserUsecase
                .Setup(x => x.ExecuteAsync(serviceUserRequest))
                .ThrowsAsync(exception);
            //Act
            var response = await _classUnderTest.UpdateServiceUserCedarNumber(serviceUserRequest);
            var statusCode = GetStatusCode(response);
            var result = GetResultData<ProblemDetails>(response);

            //Assert
            statusCode.Should().Be((int) expectedStatusCode);
            result.Status.Should().Be((int) expectedStatusCode);
            result.Detail.Should().Be(exception.Message);
        }
        private static readonly object[] _editServiceUserErrorList =
            {
                new object[]
                {
                    new ArgumentNullException(null, "message"), HttpStatusCode.NotFound
                },
                new object[]
                {
                    new ArgumentException("message"), HttpStatusCode.BadRequest
                },
                new object[]
                {
                    new InvalidOperationException("message"), HttpStatusCode.UnprocessableEntity
                }
            };


    }
}
