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

        [SetUp]
        public void Setup()
        {
            _fixture = FixtureHelpers.Fixture;
            _mockGetCurrentElementUseCase = new Mock<IGetCurrentElementsUseCase>();
            _mockGetElementByIdUseCase = new Mock<IGetElementByIdUseCase>();
            _mockProblemDetailsFactory = new MockProblemDetailsFactory();

            _classUnderTest = new ElementsController(
                _mockGetCurrentElementUseCase.Object,
                _mockGetElementByIdUseCase.Object
            );
            _classUnderTest.ProblemDetailsFactory = _mockProblemDetailsFactory.Object;
        }

        [Test]
        public async Task CanGetCurrentElements()
        {
            var elements = _fixture.BuildElement(1, 1).CreateMany();
            _mockGetCurrentElementUseCase.Setup(x => x.ExecuteAsync())
                .ReturnsAsync(elements);

            var response = await _classUnderTest.GetCurrentElements();
            var statusCode = GetStatusCode(response);
            var result = GetResultData<List<ElementResponse>>(response);

            statusCode.Should().Be((int) HttpStatusCode.OK);
            result.Should().BeEquivalentTo(elements.Select(e => e.ToResponse()));
        }

        [Test]
        public async Task CanGetElementById()
        {
            var element = _fixture.BuildElement(1, 1).Create();
            _mockGetElementByIdUseCase.Setup(x => x.ExecuteAsync(element.Id))
                .ReturnsAsync(element);

            var response = await _classUnderTest.GetElement(element.Id);
            var statusCode = GetStatusCode(response);
            var result = GetResultData<ElementResponse>(response);

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
            var response = await _classUnderTest.GetElement(elementId);
            var statusCode = GetStatusCode(response);

            // Assert
            statusCode.Should().Be((int) HttpStatusCode.NotFound);
            _mockProblemDetailsFactory.VerifyProblem(HttpStatusCode.NotFound);
        }
    }
}
