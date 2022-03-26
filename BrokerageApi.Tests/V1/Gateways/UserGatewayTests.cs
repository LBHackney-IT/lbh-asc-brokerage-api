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
    public class UserGatewayTests : DatabaseTests
    {
        private UserGateway _classUnderTest;

        [SetUp]
        public void Setup()
        {
            _classUnderTest = new UserGateway(BrokerageContext);
        }

        [Test]
        public async Task GetsUsers()
        {
            // Arrange
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

            await BrokerageContext.Users.AddAsync(careChargesOfficer);
            await BrokerageContext.Users.AddAsync(brokerageAssistant);
            await BrokerageContext.Users.AddAsync(approver);
            await BrokerageContext.Users.AddAsync(broker);
            await BrokerageContext.Users.AddAsync(deactivatedUser);
            await BrokerageContext.SaveChangesAsync();

            // Act
            var result = await _classUnderTest.GetAllAsync();

            // Assert
            Assert.That(result, Has.Count.EqualTo(4));
            Assert.That(result, Contains.Item(approver));
            Assert.That(result, Contains.Item(broker));
            Assert.That(result, Contains.Item(brokerageAssistant));
            Assert.That(result, Contains.Item(careChargesOfficer));
            Assert.That(result, Does.Not.Contain(deactivatedUser));
            Assert.That(result, Is.Ordered.Ascending.By("Name"));
        }

        [Test]
        public async Task GetsFilteredUsers()
        {
            // Arrange
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

            await BrokerageContext.Users.AddAsync(careChargesOfficer);
            await BrokerageContext.Users.AddAsync(brokerageAssistant);
            await BrokerageContext.Users.AddAsync(deactivatedUser);
            await BrokerageContext.SaveChangesAsync();

            // Act
            var result = await _classUnderTest.GetAllAsync(UserRole.BrokerageAssistant);

            // Assert
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result, Contains.Item(brokerageAssistant));
            Assert.That(result, Does.Not.Contain(careChargesOfficer));
            Assert.That(result, Does.Not.Contain(deactivatedUser));
        }
    }
}
