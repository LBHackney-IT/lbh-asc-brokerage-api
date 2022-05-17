using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using BrokerageApi.Tests.V1.Helpers;
using BrokerageApi.V1.Gateways;
using BrokerageApi.V1.Infrastructure;
using FluentAssertions;
using NUnit.Framework;

namespace BrokerageApi.Tests.V1.Gateways
{
    [TestFixture]
    public class ElementGatewayTests : DatabaseTests
    {
        private ElementGateway _classUnderTest;
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _fixture = FixtureHelpers.Fixture;

            _classUnderTest = new ElementGateway(BrokerageContext);
        }

        [Test]
        public async Task CanGetCurrentElements()
        {
            var elements = _fixture.CreateMany<Element>();
            await SeedElements(elements.ToArray());

            var resultElements = await _classUnderTest.GetCurrentAsync();

            resultElements.Should().BeEquivalentTo(elements);
        }

        [Test]
        public async Task CanGetById()
        {
            var expectedElement = _fixture.Create<Element>();
            await SeedElements(expectedElement);

            var resultElement = await _classUnderTest.GetByIdAsync(expectedElement.Id);

            resultElement.Should().BeEquivalentTo(expectedElement);
        }

        private async Task SeedElements(params Element[] elements)
        {
            await BrokerageContext.Elements.AddRangeAsync(elements);
            await BrokerageContext.SaveChangesAsync();
        }
    }
}
