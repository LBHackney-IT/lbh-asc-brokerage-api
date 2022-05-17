using System.Threading.Tasks;
using AutoFixture;
using BrokerageApi.Tests.V1.Helpers;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.UseCase;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace BrokerageApi.Tests.V1.UseCase
{
    public class GetCurrentElementsUseCaseTests
    {
        private Mock<IElementGateway> _mockElementGateway;
        private GetCurrentElementsUseCase _classUnderTest;
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _fixture = FixtureHelpers.Fixture;
            _mockElementGateway = new Mock<IElementGateway>();

            _classUnderTest = new GetCurrentElementsUseCase(_mockElementGateway.Object);
        }

        [Test]
        public async Task CanGetCurrentElements()
        {
            var elements = _fixture.BuildElement(1, 1).CreateMany();
            _mockElementGateway.Setup(x => x.GetCurrentAsync())
                .ReturnsAsync(elements);

            var resultElements = await _classUnderTest.ExecuteAsync();

            resultElements.Should().BeEquivalentTo(elements);
        }
    }
}
