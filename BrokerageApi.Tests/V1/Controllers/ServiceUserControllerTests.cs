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
        private Mock<IGetServiceOverviewUseCase> _mockGetServiceOverviewUseCase;
        private ServiceUserController _classUnderTest;
        private Fixture _fixture;
        private Mock<IGetCarePackagesByServiceUserIdUseCase> _mockGetCarePackagesByServiceUserIdUseCase;

        [SetUp]
        public void SetUp()
        {
            _fixture = FixtureHelpers.Fixture;
            _mockGetServiceOverviewUseCase = new Mock<IGetServiceOverviewUseCase>();
            _mockGetCarePackagesByServiceUserIdUseCase = new Mock<IGetCarePackagesByServiceUserIdUseCase>();

            _classUnderTest = new ServiceUserController(
                _mockGetServiceOverviewUseCase.Object,
                _mockGetCarePackagesByServiceUserIdUseCase.Object
            );

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

            var response = await _classUnderTest.GetServiceOverview(socialCareId);
            var statusCode = GetStatusCode(response);
            var result = GetResultData<List<ElementResponse>>(response);

            statusCode.Should().Be((int) HttpStatusCode.OK);
            result.Should().BeEquivalentTo(elements.Select(e => e.ToResponse()));
        }

        [Test]
        public async Task Returns404WhenOverviewNotFound()
        {
            const string socialCareId = "expectedId";
            _mockGetServiceOverviewUseCase.Setup(x => x.ExecuteAsync(socialCareId))
                .ThrowsAsync(new ArgumentException("test"));

            var response = await _classUnderTest.GetServiceOverview(socialCareId);
            var statusCode = GetStatusCode(response);
            var result = GetResultData<ProblemDetails>(response);

            statusCode.Should().Be((int) HttpStatusCode.NotFound);
            result.Status.Should().Be((int) HttpStatusCode.NotFound);
            result.Detail.Should().Be("test");
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
    }
}
