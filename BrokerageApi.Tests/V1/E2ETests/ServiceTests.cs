using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoFixture;
using BrokerageApi.Tests.V1.Helpers;
using BrokerageApi.V1.Boundary.Request;
using BrokerageApi.V1.Boundary.Response;
using BrokerageApi.V1.Factories;
using BrokerageApi.V1.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
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

    public class ServiceTests : IntegrationTests<Startup>
    {
        [SetUp]
        public void Setup()
        {
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

            await Context.Services.AddAsync(activeService);
            await Context.Services.AddAsync(parentService);
            await Context.Services.AddAsync(childService);
            await Context.Services.AddAsync(archivedService);
            await Context.SaveChangesAsync();

            // Act
            var (code, response) = await Get<List<ServiceResponse>>($"/api/v1/services");

            // Assert
            Assert.That(code, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response, Has.Count.EqualTo(3));
            Assert.That(response, Contains.Item(activeService.ToResponse()).Using(comparer));
            Assert.That(response, Contains.Item(parentService.ToResponse()).Using(comparer));
            Assert.That(response, Contains.Item(childService.ToResponse()).Using(comparer));
            Assert.That(response, Does.Not.Contain(archivedService.ToResponse()).Using(comparer));
        }
    }
}
