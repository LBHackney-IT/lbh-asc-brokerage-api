using System.Threading.Tasks;
using BrokerageApi.V1.Gateways;
using BrokerageApi.V1.Infrastructure;
using FluentAssertions;
using NUnit.Framework;

namespace BrokerageApi.Tests.V1.Gateways
{
    [TestFixture]
    public class ElementTypeGatewayTests : DatabaseTests
    {
        private ElementTypeGateway _classUnderTest;

        [SetUp]
        public void Setup()
        {
            _classUnderTest = new ElementTypeGateway(BrokerageContext);
        }

        [Test]
        public async Task GetsElementTypeById()
        {
            // Arrange
            var service = new Service
            {
                Id = 1,
                Name = "Supported Living",
                Position = 1,
                IsArchived = false,
            };

            var elementType = new ElementType
            {
                Id = 1,
                ServiceId = 1,
                Name = "Day Opportunities (daily)",
                CostType = ElementCostType.Daily,
                NonPersonalBudget = false,
                IsArchived = false
            };

            await BrokerageContext.Services.AddAsync(service);
            await BrokerageContext.ElementTypes.AddAsync(elementType);
            await BrokerageContext.SaveChangesAsync();

            // Act
            var result = await _classUnderTest.GetByIdAsync(elementType.Id);

            // Assert
            result.Should().BeEquivalentTo(elementType);
        }
    }
}
