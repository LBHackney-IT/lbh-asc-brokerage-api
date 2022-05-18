using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoFixture;
using BrokerageApi.Tests.V1.Controllers.Mocks;
using BrokerageApi.Tests.V1.Helpers;
using BrokerageApi.V1.Boundary.Response;
using BrokerageApi.V1.Controllers;
using BrokerageApi.V1.Factories;
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
        private MockProblemDetailsFactory _mockProblemDetailsFactory;

        [SetUp]
        public void SetUp()
        {
            _fixture = FixtureHelpers.Fixture;
            _mockGetServiceOverviewUseCase = new Mock<IGetServiceOverviewUseCase>();
            _mockProblemDetailsFactory = new MockProblemDetailsFactory();

            _classUnderTest = new ServiceUserController(_mockGetServiceOverviewUseCase.Object);
            _classUnderTest.ProblemDetailsFactory = _mockProblemDetailsFactory.Object;
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
    }
}
