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
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace BrokerageApi.Tests.V1.Controllers
{
    public class ServiceUserControllerTests : ControllerTests
    {
        private Mock<IGetServiceOverviewsUseCase> _mockGetServiceOverviewUseCase;
        private ServiceUserController _classUnderTest;
        private Fixture _fixture;
        private Mock<IGetCarePackagesByServiceUserIdUseCase> _mockGetCarePackagesByServiceUserIdUseCase;

        private Mock<IGetServiceUserByRequestUseCase> _mockGetServiceUserByRequestUseCase;

        [SetUp]
        public void SetUp()
        {
            _fixture = FixtureHelpers.Fixture;
            _mockGetServiceOverviewUseCase = new Mock<IGetServiceOverviewsUseCase>();
            _mockGetCarePackagesByServiceUserIdUseCase = new Mock<IGetCarePackagesByServiceUserIdUseCase>();
            _mockGetServiceUserByRequestUseCase = new Mock<IGetServiceUserByRequestUseCase>();

            _classUnderTest = new ServiceUserController(
                _mockGetServiceOverviewUseCase.Object,
                _mockGetCarePackagesByServiceUserIdUseCase.Object,
                _mockGetServiceUserByRequestUseCase.Object
                );
            SetupAuthentication(_classUnderTest);

            SetupAuthentication(_classUnderTest);
        }

        [Test]
        public async Task CanGetServiceOverviews()
        {
            const string socialCareId = "expectedId";
            var serviceOverviews = _fixture.BuildServiceOverview().CreateMany();
            _mockGetServiceOverviewUseCase.Setup(x => x.ExecuteAsync(socialCareId))
                .ReturnsAsync(serviceOverviews);

            var response = await _classUnderTest.GetServiceOverviews(socialCareId);
            var statusCode = GetStatusCode(response);
            var result = GetResultData<List<ServiceOverviewResponse>>(response);

            statusCode.Should().Be((int) HttpStatusCode.OK);
            result.Should().BeEquivalentTo(serviceOverviews.Select(e => e.ToResponse()));
        }

        [Test]
        public async Task Returns404WhenOverviewNotFound()
        {
            const string socialCareId = "expectedId";
            _mockGetServiceOverviewUseCase
                .Setup(x => x.ExecuteAsync(socialCareId))
                .Callback((string socialCareId) => throw new ArgumentNullException(nameof(socialCareId), "Service user not found for: expectedId"))
                .Returns(Task.FromResult(new List<ServiceOverview>() as IEnumerable<ServiceOverview>));

            var response = await _classUnderTest.GetServiceOverviews(socialCareId);
            var statusCode = GetStatusCode(response);
            var result = GetResultData<ProblemDetails>(response);

            statusCode.Should().Be((int) HttpStatusCode.NotFound);
            result.Status.Should().Be((int) HttpStatusCode.NotFound);
            result.Detail.Should().Be("Service user not found for: expectedId (Parameter 'socialCareId')");
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
    }
}
