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
    public class UserResponseComparer : IEqualityComparer<UserResponse>
    {
        public bool Equals(UserResponse u1, UserResponse u2)
        {
            return u1.Id == u2.Id;
        }

        public int GetHashCode(UserResponse u)
        {
            return u.Id.GetHashCode();
        }
    }

    public class UserTests : IntegrationTests<Startup>
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test, Property("AsUser", "BrokerageAssistant")]
        public async Task CanGetAllUsers()
        {
            // Arrange
            var comparer = new UserResponseComparer();

            var careChargesOfficer = new User()
            {
                Name = "Care Charges Officer",
                Email = "care.chargesofficer@hackney.gov.uk",
                IsActive = true,
                Roles = new List<UserRole>() {
                    UserRole.CareChargesOfficer
                }
            };

            var brokerageAssistant = new User()
            {
                Name = "Brokerage Assistant",
                Email = "brokerage.assistant@hackney.gov.uk",
                IsActive = true,
                Roles = new List<UserRole>() {
                    UserRole.BrokerageAssistant
                }
            };

            var approver = new User()
            {
                Name = "An Approver",
                Email = "an.approver@hackney.gov.uk",
                IsActive = true,
                Roles = new List<UserRole>() {
                    UserRole.Approver
                }
            };

            var broker = new User()
            {
                Name = "A Broker",
                Email = "a.broker@hackney.gov.uk",
                IsActive = true,
                Roles = new List<UserRole>() {
                    UserRole.Broker
                }
            };

            var deactivatedUser = new User()
            {
                Name = "Deactivated User",
                Email = "deactivated.user@hackney.gov.uk",
                IsActive = false,
                Roles = new List<UserRole>()
            };

            await Context.Users.AddAsync(careChargesOfficer);
            await Context.Users.AddAsync(brokerageAssistant);
            await Context.Users.AddAsync(approver);
            await Context.Users.AddAsync(broker);
            await Context.Users.AddAsync(deactivatedUser);
            await Context.SaveChangesAsync();

            // Act
            var (code, response) = await Get<List<UserResponse>>($"/api/v1/users");

            // Assert
            Assert.That(code, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response, Has.Count.EqualTo(5)); // Add one for the API user
            Assert.That(response, Contains.Item(approver.ToResponse()).Using(comparer));
            Assert.That(response, Contains.Item(broker.ToResponse()).Using(comparer));
            Assert.That(response, Contains.Item(brokerageAssistant.ToResponse()).Using(comparer));
            Assert.That(response, Contains.Item(careChargesOfficer.ToResponse()).Using(comparer));
            Assert.That(response, Does.Not.Contain(deactivatedUser.ToResponse()).Using(comparer));
            Assert.That(response, Is.Ordered.Ascending.By("Name"));
        }

        [Test, Property("AsUser", "BrokerageAssistant")]
        public async Task CanGetFilteredAllUsers()
        {
            // Arrange
            var comparer = new UserResponseComparer();

            var careChargesOfficer = new User()
            {
                Name = "Care Charges Officer",
                Email = "care.chargesofficer@hackney.gov.uk",
                IsActive = true,
                Roles = new List<UserRole>() {
                    UserRole.CareChargesOfficer
                }
            };

            var brokerageAssistant = new User()
            {
                Name = "Brokerage Assistant",
                Email = "brokerage.assistant@hackney.gov.uk",
                IsActive = true,
                Roles = new List<UserRole>() {
                    UserRole.BrokerageAssistant
                }
            };

            var deactivatedUser = new User()
            {
                Name = "Deactivated User",
                Email = "deactivated.user@hackney.gov.uk",
                IsActive = false,
                Roles = new List<UserRole>()
            };

            await Context.Users.AddAsync(careChargesOfficer);
            await Context.Users.AddAsync(brokerageAssistant);
            await Context.Users.AddAsync(deactivatedUser);
            await Context.SaveChangesAsync();

            // Act
            var (code, response) = await Get<List<UserResponse>>($"/api/v1/users?role=BrokerageAssistant");

            // Assert
            Assert.That(code, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response, Has.Count.EqualTo(2)); // Add one for the API user
            Assert.That(response, Contains.Item(brokerageAssistant.ToResponse()).Using(comparer));
            Assert.That(response, Does.Not.Contain(careChargesOfficer.ToResponse()).Using(comparer));
            Assert.That(response, Does.Not.Contain(deactivatedUser.ToResponse()).Using(comparer));
        }
    }
}
