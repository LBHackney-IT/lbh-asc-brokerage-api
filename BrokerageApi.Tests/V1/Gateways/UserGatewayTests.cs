using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using BrokerageApi.Tests.V1.Helpers;
using BrokerageApi.V1.Gateways;
using BrokerageApi.V1.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
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
                Roles = new List<UserRole>()
                {
                    UserRole.CareChargesOfficer
                }
            };

            var brokerageAssistant = new User()
            {
                Name = "Brokerage Assistant",
                Email = "brokerage.assistant@hackney.gov.uk",
                IsActive = true,
                Roles = new List<UserRole>()
                {
                    UserRole.BrokerageAssistant
                }
            };

            var approver = new User()
            {
                Name = "An Approver",
                Email = "an.approver@hackney.gov.uk",
                IsActive = true,
                Roles = new List<UserRole>()
                {
                    UserRole.Approver
                }
            };

            var broker = new User()
            {
                Name = "A Broker",
                Email = "a.broker@hackney.gov.uk",
                IsActive = true,
                Roles = new List<UserRole>()
                {
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
                Roles = new List<UserRole>()
                {
                    UserRole.CareChargesOfficer
                }
            };

            var brokerageAssistant = new User()
            {
                Name = "Brokerage Assistant",
                Email = "brokerage.assistant@hackney.gov.uk",
                IsActive = true,
                Roles = new List<UserRole>()
                {
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

        [Test]
        public async Task GetsUserByEmail()
        {
            // Arrange
            var user = new User()
            {
                Name = "Brokerage Assistant",
                Email = "brokerage.assistant@hackney.gov.uk",
                IsActive = true,
                Roles = new List<UserRole>()
                {
                    UserRole.BrokerageAssistant
                }
            };

            await BrokerageContext.Users.AddAsync(user);
            await BrokerageContext.SaveChangesAsync();

            // Act
            var result = await _classUnderTest.GetByEmailAsync("brokerage.assistant@hackney.gov.uk");

            // Assert
            Assert.That(result, Is.EqualTo(user));
        }

        [Test]
        public async Task DoesNotGetUserByEmailWhenDeactivated()
        {
            // Arrange
            var user = new User()
            {
                Name = "Deactivated User",
                Email = "deactivated.user@hackney.gov.uk",
                IsActive = false,
                Roles = new List<UserRole>()
            };

            await BrokerageContext.Users.AddAsync(user);
            await BrokerageContext.SaveChangesAsync();

            // Act
            var result = await _classUnderTest.GetByEmailAsync("deactivated.user@hackney.gov.uk");

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task CanCreateUser()
        {
            const string expectedName = "Expected Name";
            const string expectedEmail = "expected@email.com";

            var result = await _classUnderTest.CreateUser(expectedEmail, expectedName);

            var user = await BrokerageContext.Users.SingleOrDefaultAsync(u => u.Email == expectedEmail);
            user.Name.Should().Be(expectedName);
            user.Roles.Should().BeNullOrEmpty();
            user.IsActive.Should().BeTrue();
            result.Should().Be(user);
        }

        [Test]
        public async Task CreateUserThrowsWhenUserExists()
        {
            const string expectedEmail = "expected@email.com";
            var user = Fixture.BuildUser()
                .With(u => u.Email, expectedEmail)
                .Create();

            await BrokerageContext.Users.AddAsync(user);
            await BrokerageContext.SaveChangesAsync();

            Func<Task> act = () => _classUnderTest.CreateUser(expectedEmail, "");

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage($"User with email address {expectedEmail} already exists");
        }

        [Test]
        public async Task GetApproversWithLimit()
        {
            const decimal approvalLimit = 100;
            var approversAboveLimit = Fixture.Build<User>()
                .With(u => u.IsActive, true)
                .With(u => u.Roles, new List<UserRole>
                {
                    UserRole.Approver
                })
                .With(u => u.ApprovalLimit, Fixture.CreateInt((int) approvalLimit, 100000))
                .CreateMany();
            var approversBelowLimit = Fixture.Build<User>()
                .With(u => u.IsActive, true)
                .With(u => u.Roles, new List<UserRole>
                {
                    UserRole.Approver
                })
                .With(u => u.ApprovalLimit, Fixture.CreateInt(0, (int) approvalLimit - 1))
                .CreateMany();
            var nonApprovers = Fixture.Build<User>()
                .With(u => u.IsActive, true)
                .With(u => u.Roles, new List<UserRole>
                {
                    UserRole.Broker
                })
                .Without(u => u.ApprovalLimit)
                .CreateMany();

            await BrokerageContext.Users.AddRangeAsync(approversAboveLimit);
            await BrokerageContext.Users.AddRangeAsync(approversBelowLimit);
            await BrokerageContext.Users.AddRangeAsync(nonApprovers);
            await BrokerageContext.SaveChangesAsync();

            var result = await _classUnderTest.GetBudgetApproversAsync(approvalLimit);

            result.Should().Contain(approversAboveLimit);
            result.Should().NotContain(approversBelowLimit);
            result.Should().NotContain(nonApprovers);
        }
    }
}
