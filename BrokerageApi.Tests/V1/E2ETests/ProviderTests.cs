using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using BrokerageApi.V1.Boundary.Response;
using BrokerageApi.V1.Factories;
using BrokerageApi.V1.Infrastructure;
using NUnit.Framework;

namespace BrokerageApi.Tests.V1.E2ETests
{
    public class ProviderResponseComparer : IEqualityComparer<ProviderResponse>
    {
        public bool Equals(ProviderResponse p1, ProviderResponse p2)
        {
            return p1.Id == p2.Id;
        }

        public int GetHashCode(ProviderResponse p)
        {
            return p.Id.GetHashCode();
        }
    }

    public class ProviderTests : IntegrationTests<Startup>
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test, Property("AsUser", "Broker")]
        public async Task CanFindProviders()
        {
            // Arrange
            var comparer = new ProviderResponseComparer();

            var provider = new Provider()
            {
                Id = 1,
                Name = "Acme Homes",
                Address = "1 Knowhere Road",
                Type = ProviderType.Framework
            };

            var otherProvider = new Provider()
            {
                Id = 2,
                Name = "Better Homes",
                Address = "99 Knowhere Road",
                Type = ProviderType.Framework
            };

            await Context.Providers.AddAsync(provider);
            await Context.Providers.AddAsync(otherProvider);
            await Context.SaveChangesAsync();

            Context.ChangeTracker.Clear();

            // Act
            var (code, response) = await Get<List<ProviderResponse>>($"/api/v1/providers?query=Acme");

            // Assert
            Assert.That(code, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response, Has.Count.EqualTo(1));
            Assert.That(response, Contains.Item(provider.ToResponse()).Using(comparer));
            Assert.That(response, Does.Not.Contain(otherProvider.ToResponse()).Using(comparer));
        }

        [Test, Property("AsUser", "Broker")]
        public async Task CanFindProvidersByPartialName()
        {
            // Arrange
            var comparer = new ProviderResponseComparer();

            var provider = new Provider()
            {
                Id = 1,
                Name = "Hartwig Care Limited",
                Address = "1 Knowhere Road",
                Type = ProviderType.Framework
            };

            var otherProvider = new Provider()
            {
                Id = 2,
                Name = "Better Homes",
                Address = "99 Knowhere Road",
                Type = ProviderType.Framework
            };

            await Context.Providers.AddAsync(provider);
            await Context.Providers.AddAsync(otherProvider);
            await Context.SaveChangesAsync();

            Context.ChangeTracker.Clear();

            // Act
            var (code, response) = await Get<List<ProviderResponse>>($"/api/v1/providers?query=hart+care");

            // Assert
            Assert.That(code, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response, Has.Count.EqualTo(1));
            Assert.That(response, Contains.Item(provider.ToResponse()).Using(comparer));
            Assert.That(response, Does.Not.Contain(otherProvider.ToResponse()).Using(comparer));
        }
    }
}
