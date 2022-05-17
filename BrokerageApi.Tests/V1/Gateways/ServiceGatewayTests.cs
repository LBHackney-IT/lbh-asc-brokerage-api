using System.Collections.Generic;
using System.Threading.Tasks;
using BrokerageApi.V1.Gateways;
using BrokerageApi.V1.Infrastructure;
using FluentAssertions;
using NUnit.Framework;

namespace BrokerageApi.Tests.V1.Gateways
{
    [TestFixture]
    public class ServiceGatewayTests : DatabaseTests
    {
        private ServiceGateway _classUnderTest;

        [SetUp]
        public void Setup()
        {
            _classUnderTest = new ServiceGateway(BrokerageContext);
        }

        [Test]
        public async Task GetsServices()
        {
            // Arrange
            var activeService = new Service()
            {
                Id = 1,
                Name = "Shared Lives",
                Position = 1,
                IsArchived = false
            };

            var parentService = new Service()
            {
                Id = 2,
                Name = "Residential Care",
                IsArchived = false,
                Position = 2,
            };

            var childService = new Service()
            {
                Id = 3,
                ParentId = 2,
                Name = "Long Stay Residential Care",
                Position = 1,
                IsArchived = false
            };

            var archivedService = new Service()
            {
                Id = 1000,
                Name = "Legacy Service",
                Position = 1000,
                IsArchived = true
            };

            await BrokerageContext.Services.AddAsync(activeService);
            await BrokerageContext.Services.AddAsync(parentService);
            await BrokerageContext.Services.AddAsync(childService);
            await BrokerageContext.Services.AddAsync(archivedService);
            await BrokerageContext.SaveChangesAsync();

            // Act
            var result = await _classUnderTest.GetAllAsync();

            // Assert
            Assert.That(result, Has.Count.EqualTo(3));
            Assert.That(result, Contains.Item(activeService));
            Assert.That(result, Contains.Item(parentService));
            Assert.That(result, Contains.Item(childService));
            Assert.That(result, Does.Not.Contain(archivedService));
        }

        [Test]
        public async Task GetsServiceById()
        {
            // Arrange
            var service = new Service
            {
                Id = 1,
                Name = "Supported Living",
                Position = 1,
                IsArchived = false,
                ElementTypes = new List<ElementType>
                {
                    new ElementType
                    {
                        Id = 1,
                        Name = "Day Opportunities (daily)",
                        CostType = ElementCostType.Daily,
                        NonPersonalBudget = false,
                        IsArchived = false
                    }
                }
            };

            await BrokerageContext.Services.AddAsync(service);
            await BrokerageContext.SaveChangesAsync();

            // Act
            var result = await _classUnderTest.GetByIdAsync(service.Id);

            // Assert
            result.Should().BeEquivalentTo(service);
        }
    }
}
