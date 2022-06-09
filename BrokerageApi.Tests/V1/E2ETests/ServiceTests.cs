using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoFixture;
using BrokerageApi.Tests.V1.Helpers;
using BrokerageApi.V1.Boundary.Response;
using BrokerageApi.V1.Factories;
using BrokerageApi.V1.Infrastructure;
using FluentAssertions;
using NUnit.Framework;

namespace BrokerageApi.Tests.V1.E2ETests
{
    public class ServiceResponseComparer : IEqualityComparer<ServiceResponse>
    {
        public bool Equals(ServiceResponse s1, ServiceResponse s2)
        {
            return s1.Id == s2.Id;
        }

        public int GetHashCode(ServiceResponse s)
        {
            return s.Id.GetHashCode();
        }
    }

    public class ElementTypeResponseComparer : IEqualityComparer<ElementTypeResponse>
    {
        public bool Equals(ElementTypeResponse et1, ElementTypeResponse et2)
        {
            return et1.Id == et2.Id;
        }

        public int GetHashCode(ElementTypeResponse et)
        {
            return et.Id.GetHashCode();
        }
    }

    public class ServiceTests : IntegrationTests<Startup>
    {
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _fixture = FixtureHelpers.Fixture;
        }

        [Test, Property("AsUser", "Broker")]
        public async Task CanGetAllServices()
        {
            // Arrange
            var comparer = new ServiceResponseComparer();

            var activeService = new Service()
            {
                Id = 1,
                Name = "Shared Lives",
                Position = 1,
                IsArchived = false,
                ElementTypes = _fixture.BuildElementType(1).CreateMany().ToList()
            };

            var parentService = new Service()
            {
                Id = 2,
                Name = "Residential Care",
                IsArchived = false,
                Position = 2
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

            await Context.Services.AddAsync(activeService);
            await Context.Services.AddAsync(parentService);
            await Context.Services.AddAsync(childService);
            await Context.Services.AddAsync(archivedService);
            await Context.SaveChangesAsync();

            Context.ChangeTracker.Clear();

            // Act
            var (code, response) = await Get<List<ServiceResponse>>($"/api/v1/services");

            // Assert
            Assert.That(code, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response, Has.Count.EqualTo(3));
            Assert.That(response, Contains.Item(activeService.ToResponse()).Using(comparer));
            Assert.That(response, Contains.Item(parentService.ToResponse()).Using(comparer));
            Assert.That(response, Contains.Item(childService.ToResponse()).Using(comparer));
            Assert.That(response, Does.Not.Contain(archivedService.ToResponse()).Using(comparer));

            var resultService = response.Single(s => s.Id == activeService.Id);
            resultService.ElementTypes.Should().BeEquivalentTo(activeService.ElementTypes.Select(et => et.ToResponse()));
        }

        [Test, Property("AsUser", "Broker")]
        public async Task CanGetService()
        {
            // Arrange
            var serviceComparer = new ServiceResponseComparer();
            var elementTypeComparer = new ElementTypeResponseComparer();

            var service = new Service()
            {
                Id = 1,
                Name = "Supported Living",
                Position = 1,
                IsArchived = false
            };

            var legacyElementType = new ElementType
            {
                Id = 1,
                ServiceId = 1,
                Name = "Legacy Element Type",
                CostType = ElementCostType.Daily,
                NonPersonalBudget = false,
                IsArchived = true
            };

            var activeElementType = new ElementType
            {
                Id = 2,
                ServiceId = 1,
                Name = "Day Opportunities (daily)",
                CostType = ElementCostType.Daily,
                NonPersonalBudget = false,
                IsArchived = false
            };

            await Context.Services.AddAsync(service);
            await Context.ElementTypes.AddAsync(legacyElementType);
            await Context.ElementTypes.AddAsync(activeElementType);
            await Context.SaveChangesAsync();

            Context.ChangeTracker.Clear();

            // Act
            var (code, response) = await Get<ServiceResponse>($"/api/v1/services/{service.Id}");

            // Assert
            Assert.That(code, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response, Is.EqualTo(service.ToResponse()).Using(serviceComparer));
            Assert.That(response.ElementTypes, Has.Count.EqualTo(1));
            Assert.That(response.ElementTypes, Contains.Item(activeElementType.ToResponse()).Using(elementTypeComparer));
            Assert.That(response.ElementTypes, Does.Not.Contain(legacyElementType.ToResponse()).Using(elementTypeComparer));
        }
    }
}
