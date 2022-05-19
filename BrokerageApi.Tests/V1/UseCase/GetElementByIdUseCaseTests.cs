using System;
using System.Threading.Tasks;
using AutoFixture;
using BrokerageApi.Tests.V1.Helpers;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.UseCase;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace BrokerageApi.Tests.V1.UseCase
{
    public class GetElementByIdUseCaseTests
    {
        private Mock<IElementGateway> _mockElementGateway;
        private GetElementByIdUseCase _classUnderTest;
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _fixture = FixtureHelpers.Fixture;
            _mockElementGateway = new Mock<IElementGateway>();

            _classUnderTest = new GetElementByIdUseCase(_mockElementGateway.Object);
        }

        [Test]
        public async Task CanGetCurrentElements()
        {
            var element = _fixture.BuildElement(1, 1).Create();
            _mockElementGateway.Setup(x => x.GetByIdAsync(element.Id))
                .ReturnsAsync(element);

            var resultElement = await _classUnderTest.ExecuteAsync(element.Id);

            resultElement.Should().BeEquivalentTo(element);
        }

        [Test]
        public void ThrowsExceptionWhenElementNotFound()
        {
            const int elementId = 123;
            _mockElementGateway.Setup(x => x.GetByIdAsync(elementId))
                .ReturnsAsync((Element) null);

            Func<Task<Element>> act = () => _classUnderTest.ExecuteAsync(elementId);

            act.Should().ThrowAsync<ArgumentException>()
                .WithMessage($"Element not found for: {elementId}");
        }
    }
}
