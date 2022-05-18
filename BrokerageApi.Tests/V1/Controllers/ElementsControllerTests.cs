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
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;

namespace BrokerageApi.Tests.V1.Controllers
{
    [TestFixture]
    public class ElementsControllerTests : ControllerTests
    {
        private Fixture _fixture;
        private Mock<IGetCurrentElementsUseCase> _mockGetCurrentElementUseCase;
        private Mock<IGetElementByIdUseCase> _mockGetElementByIdUseCase;
        private ElementsController _classUnderTest;
        private MockProblemDetailsFactory _mockProblemDetailsFactory;
        private Mock<IEndElementUseCase> _mockEndElementUseCase;

        [SetUp]
        public void Setup()
        {
            _fixture = FixtureHelpers.Fixture;
            _mockGetCurrentElementUseCase = new Mock<IGetCurrentElementsUseCase>();
            _mockGetElementByIdUseCase = new Mock<IGetElementByIdUseCase>();
            _mockProblemDetailsFactory = new MockProblemDetailsFactory();
            _mockEndElementUseCase = new Mock<IEndElementUseCase>();

            _classUnderTest = new ElementsController(
                _mockGetCurrentElementUseCase.Object,
                _mockGetElementByIdUseCase.Object,
                _mockEndElementUseCase.Object
            );
            _classUnderTest.ProblemDetailsFactory = _mockProblemDetailsFactory.Object;
        }

        [Test]
        public async Task CanGetCurrentElements()
        {
            var elements = _fixture.BuildElement(1, 1).CreateMany();
            _mockGetCurrentElementUseCase.Setup(x => x.ExecuteAsync())
                .ReturnsAsync(elements);

            var objectResult = await _classUnderTest.GetCurrentElements();
            var statusCode = GetStatusCode(objectResult);
            var result = GetResultData<List<ElementResponse>>(objectResult);

            statusCode.Should().Be((int) HttpStatusCode.OK);
            result.Should().BeEquivalentTo(elements.Select(e => e.ToResponse()));
        }

        [Test]
        public async Task CanGetElementById()
        {
            var element = _fixture.BuildElement(1, 1).Create();
            _mockGetElementByIdUseCase.Setup(x => x.ExecuteAsync(element.Id))
                .ReturnsAsync(element);

            var objectResult = await _classUnderTest.GetElement(element.Id);
            var statusCode = GetStatusCode(objectResult);
            var result = GetResultData<ElementResponse>(objectResult);

            statusCode.Should().Be((int) HttpStatusCode.OK);
            result.Should().BeEquivalentTo(element.ToResponse());
        }

        [Test]
        public async Task Returns404WhenElementNotFound()
        {
            // Arrange
            const int elementId = 123;
            _mockGetElementByIdUseCase.Setup(x => x.ExecuteAsync(elementId))
                .ThrowsAsync(new ArgumentException("test"));

            // Act
            var objectResult = await _classUnderTest.GetElement(elementId);
            var statusCode = GetStatusCode(objectResult);

            // Assert
            statusCode.Should().Be((int) HttpStatusCode.NotFound);
            _mockProblemDetailsFactory.VerifyProblem(HttpStatusCode.NotFound);
        }

        [Test]
        public async Task CanEndElement()
        {
            const int elementId = 1234;
            var request = _fixture.Create<EndElementRequest>();

            var response = await _classUnderTest.EndElement(elementId, request);
            var statusCode = GetStatusCode(response);

            _mockEndElementUseCase.Verify(x => x.ExecuteAsync(elementId, request.EndDate));
            statusCode.Should().Be((int) HttpStatusCode.OK);
        }

        private static readonly object[] _endElementErrors =
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

        [TestCaseSource(nameof(_endElementErrors)), Property("AsUser", "Broker")]
        public async Task EndElementMapsErrors(Exception exception, HttpStatusCode expectedStatusCode)
        {
            const int elementId = 1234;
            var request = _fixture.Create<EndElementRequest>();
            _mockEndElementUseCase.Setup(x => x.ExecuteAsync(elementId, request.EndDate))
                .ThrowsAsync(exception);

            var response = await _classUnderTest.EndElement(elementId, request);
            var statusCode = GetStatusCode(response);

            statusCode.Should().Be((int) expectedStatusCode);
            _mockProblemDetailsFactory.VerifyStatusCode(expectedStatusCode);
        }
    }
}
