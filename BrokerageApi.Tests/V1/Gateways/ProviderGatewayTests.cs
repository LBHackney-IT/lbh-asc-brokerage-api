using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BrokerageApi.V1.Gateways;
using BrokerageApi.V1.Infrastructure;
using FluentAssertions;
using FluentAssertions.Extensions;
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
        public async Task FindsProvidersByServiceAndName()
        {
            // Arrange
            var service = new Service()
            {
                Id = 1,
                Name = "Shared Lives",
                Position = 1
            };

            var provider = new Provider()
            {
                Id = 1,
                Name = "Acme Homes",
                Address = "1 Knowhere Road",
                Type = ProviderType.Framework
            };

            var providerService = new ProviderService()
            {
                ProviderId = 1,
                ServiceId = 1,
                SubjectiveCode = "599999"
            };

            await BrokerageContext.Services.AddAsync(service);
            await BrokerageContext.Providers.AddAsync(provider);
            await BrokerageContext.ProviderServices.AddAsync(providerService);
            await BrokerageContext.SaveChangesAsync();

            // Act
            var result = await _classUnderTest.FindByServiceIdAsync(service.Id, "Acme");

            // Assert
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result, Contains.Item(provider));
        }

        [Test]
        public async Task FindsProvidersByServiceAndAddress()
        {
            // Arrange
            var service = new Service()
            {
                Id = 1,
                Name = "Shared Lives",
                Position = 1
            };

            var provider = new Provider()
            {
                Id = 1,
                Name = "Acme Homes",
                Address = "1 Knowhere Road",
                Type = ProviderType.Framework
            };

            var providerService = new ProviderService()
            {
                ProviderId = 1,
                ServiceId = 1,
                SubjectiveCode = "599999"
            };

            await BrokerageContext.Services.AddAsync(service);
            await BrokerageContext.Providers.AddAsync(provider);
            await BrokerageContext.ProviderServices.AddAsync(providerService);
            await BrokerageContext.SaveChangesAsync();

            // Act
            var result = await _classUnderTest.FindByServiceIdAsync(service.Id, "Knowhere");

            // Assert
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result, Contains.Item(provider));
        }

        [Test]
        public async Task FindsProvidersByServiceNameAndAddress()
        {
            // Arrange
            var service = new Service()
            {
                Id = 1,
                Name = "Shared Lives",
                Position = 1
            };

            var provider = new Provider()
            {
                Id = 1,
                Name = "Acme Homes",
                Address = "1 Knowhere Road",
                Type = ProviderType.Framework
            };

            var providerService = new ProviderService()
            {
                ProviderId = 1,
                ServiceId = 1,
                SubjectiveCode = "599999"
            };

            await BrokerageContext.Services.AddAsync(service);
            await BrokerageContext.Providers.AddAsync(provider);
            await BrokerageContext.ProviderServices.AddAsync(providerService);
            await BrokerageContext.SaveChangesAsync();

            // Act
            var result = await _classUnderTest.FindByServiceIdAsync(service.Id, "Acme Knowhere");

            // Assert
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result, Contains.Item(provider));
        }

        [Test]
        public async Task DoesNotFindArchivedProviders()
        {
            // Arrange
            var service = new Service()
            {
                Id = 1,
                Name = "Shared Lives",
                Position = 1
            };

            var provider = new Provider()
            {
                Id = 1,
                Name = "Acme Homes",
                Address = "1 Knowhere Road",
                Type = ProviderType.Framework,
                IsArchived = true
            };

            var providerService = new ProviderService()
            {
                ProviderId = 1,
                ServiceId = 1,
                SubjectiveCode = "599999"
            };

            await BrokerageContext.Services.AddAsync(service);
            await BrokerageContext.Providers.AddAsync(provider);
            await BrokerageContext.ProviderServices.AddAsync(providerService);
            await BrokerageContext.SaveChangesAsync();

            // Act
            var result = await _classUnderTest.FindByServiceIdAsync(service.Id, "Acme");

            // Assert
            Assert.That(result, Has.Count.EqualTo(0));
        }

        [Test]
        public async Task DoesNotFindProvidersWithOtherName()
        {
            // Arrange
            var service = new Service()
            {
                Id = 1,
                Name = "Shared Lives",
                Position = 1,
            };

            var provider = new Provider()
            {
                Id = 1,
                Name = "Acme Homes",
                Address = "1 Knowhere Road",
                Type = ProviderType.Framework
            };

            var providerService = new ProviderService()
            {
                ProviderId = 1,
                ServiceId = 1,
                SubjectiveCode = "599999"
            };

            await BrokerageContext.Services.AddAsync(service);
            await BrokerageContext.Providers.AddAsync(provider);
            await BrokerageContext.ProviderServices.AddAsync(providerService);
            await BrokerageContext.SaveChangesAsync();

            // Act
            var result = await _classUnderTest.FindByServiceIdAsync(service.Id, "FooBar");

            // Assert
            Assert.That(result, Has.Count.EqualTo(0));
        }

        [Test]
        public async Task DoesNotFindProvidersForOtherServices()
        {
            // Arrange
            var service = new Service()
            {
                Id = 1,
                Name = "Shared Lives",
                Position = 1,
            };

            var otherService = new Service()
            {
                Id = 2,
                Name = "Supported Living",
                Position = 2,
            };

            var provider = new Provider()
            {
                Id = 1,
                Name = "Acme Homes",
                Address = "1 Knowhere Road",
                Type = ProviderType.Framework,
                IsArchived = true
            };

            var providerService = new ProviderService()
            {
                ProviderId = 1,
                ServiceId = 2,
                SubjectiveCode = "599999"
            };

            await BrokerageContext.Services.AddAsync(service);
            await BrokerageContext.Services.AddAsync(otherService);
            await BrokerageContext.Providers.AddAsync(provider);
            await BrokerageContext.ProviderServices.AddAsync(providerService);
            await BrokerageContext.SaveChangesAsync();

            // Act
            var result = await _classUnderTest.FindByServiceIdAsync(service.Id, "Acme");

            // Assert
            Assert.That(result, Has.Count.EqualTo(0));
        }
    }
}
