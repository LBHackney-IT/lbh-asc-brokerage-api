using System.Threading.Tasks;
using BrokerageApi.V1.Gateways;
using BrokerageApi.V1.Infrastructure;
using NUnit.Framework;

namespace BrokerageApi.Tests.V1.Gateways
{
    [TestFixture]
    public class ProviderGatewayTests : DatabaseTests
    {
        private ProviderGateway _classUnderTest;

        [SetUp]
        public void Setup()
        {
            _classUnderTest = new ProviderGateway(BrokerageContext);
        }

        [Test]
        public async Task FindsProvidersByName()
        {
            // Arrange
            var provider = new Provider()
            {
                Id = 1,
                Name = "Acme Homes",
                Address = "1 Knowhere Road",
                Type = ProviderType.Framework
            };

            await BrokerageContext.Providers.AddAsync(provider);
            await BrokerageContext.SaveChangesAsync();

            // Act
            var result = await _classUnderTest.FindAsync("Acme");

            // Assert
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result, Contains.Item(provider));
        }

        [Test]
        public async Task FindsProvidersByAddress()
        {
            // Arrange
            var provider = new Provider()
            {
                Id = 1,
                Name = "Acme Homes",
                Address = "1 Knowhere Road",
                Type = ProviderType.Framework
            };

            await BrokerageContext.Providers.AddAsync(provider);
            await BrokerageContext.SaveChangesAsync();

            // Act
            var result = await _classUnderTest.FindAsync("Knowhere");

            // Assert
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result, Contains.Item(provider));
        }

        [Test]
        public async Task FindsProvidersByNameAndAddress()
        {
            // Arrange
            var provider = new Provider()
            {
                Id = 1,
                Name = "Acme Homes",
                Address = "1 Knowhere Road",
                Type = ProviderType.Framework
            };

            await BrokerageContext.Providers.AddAsync(provider);
            await BrokerageContext.SaveChangesAsync();

            // Act
            var result = await _classUnderTest.FindAsync("Acme Knowhere");

            // Assert
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result, Contains.Item(provider));
        }

        [Test]
        public async Task FindsProvidersByPartialName()
        {
            // Arrange
            var provider = new Provider()
            {
                Id = 1,
                Name = "Hartwig Care Limited",
                Address = "1 Knowhere Road",
                Type = ProviderType.Framework
            };

            await BrokerageContext.Providers.AddAsync(provider);
            await BrokerageContext.SaveChangesAsync();

            // Act
            var result = await _classUnderTest.FindAsync("hart care");

            // Assert
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result, Contains.Item(provider));
        }

        [Test]
        public async Task DoesNotFindArchivedProviders()
        {
            // Arrange
            var provider = new Provider()
            {
                Id = 1,
                Name = "Acme Homes",
                Address = "1 Knowhere Road",
                Type = ProviderType.Framework,
                IsArchived = true
            };

            await BrokerageContext.Providers.AddAsync(provider);
            await BrokerageContext.SaveChangesAsync();

            // Act
            var result = await _classUnderTest.FindAsync("Acme");

            // Assert
            Assert.That(result, Has.Count.EqualTo(0));
        }

        [Test]
        public async Task DoesNotFindProvidersWithOtherName()
        {
            // Arrange
            var provider = new Provider()
            {
                Id = 1,
                Name = "Acme Homes",
                Address = "1 Knowhere Road",
                Type = ProviderType.Framework
            };

            await BrokerageContext.Providers.AddAsync(provider);
            await BrokerageContext.SaveChangesAsync();

            // Act
            var result = await _classUnderTest.FindAsync("FooBar");

            // Assert
            Assert.That(result, Has.Count.EqualTo(0));
        }

        [Test]
        public async Task ReturnsEmptyListIfNullQuery()
        {
            // Arrange
            var provider = new Provider()
            {
                Id = 1,
                Name = "Acme Homes",
                Address = "1 Knowhere Road",
                Type = ProviderType.Framework
            };

            await BrokerageContext.Providers.AddAsync(provider);
            await BrokerageContext.SaveChangesAsync();

            // Act
            var result = await _classUnderTest.FindAsync(null);

            // Assert
            Assert.That(result, Has.Count.EqualTo(0));
        }

        [Test]
        public async Task ReturnsEmptyListIfEmptyQuery()
        {
            // Arrange
            var provider = new Provider()
            {
                Id = 1,
                Name = "Acme Homes",
                Address = "1 Knowhere Road",
                Type = ProviderType.Framework
            };

            await BrokerageContext.Providers.AddAsync(provider);
            await BrokerageContext.SaveChangesAsync();

            // Act
            var result = await _classUnderTest.FindAsync("");

            // Assert
            Assert.That(result, Has.Count.EqualTo(0));
        }

        [Test]
        public async Task ReturnsEmptyListIfWhitespaceQuery()
        {
            // Arrange
            var provider = new Provider()
            {
                Id = 1,
                Name = "Acme Homes",
                Address = "1 Knowhere Road",
                Type = ProviderType.Framework
            };

            await BrokerageContext.Providers.AddAsync(provider);
            await BrokerageContext.SaveChangesAsync();

            // Act
            var result = await _classUnderTest.FindAsync("  ");

            // Assert
            Assert.That(result, Has.Count.EqualTo(0));
        }
    }
}
