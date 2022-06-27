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
    public class ServiceUserGatewayTests : DatabaseTests
    {
        private ServiceUserGateway _classUnderTest;

        [SetUp]
        public void Setup()
        {
            _classUnderTest = new ServiceUserGateway(BrokerageContext);
        }

        [Test]
        public async Task GetsExpectedServiceUsersById()
        {
            // Arrange
            var serviceUser = Fixture.BuildServiceUser().Create();
            var anotherServiceUser = Fixture.BuildServiceUser().Create();

            await BrokerageContext.ServiceUsers.AddAsync(serviceUser);
            await BrokerageContext.ServiceUsers.AddAsync(anotherServiceUser);
            await BrokerageContext.SaveChangesAsync();

            var request = Fixture.BuildServiceUserRequest(anotherServiceUser.SocialCareId).Create();

            // Act
            var result = await _classUnderTest.GetByRequestAsync(request);

            // Assert
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result, Contains.Item(anotherServiceUser));
        }
        [Test]
        public async Task GetsExpectedServiceUsersByName()
        {
            // Arrange
            var serviceUser = Fixture.BuildServiceUser().Create();
            var anotherServiceUser = Fixture.BuildServiceUser()
            .With(su => su.ServiceUserName, "Fake Person")
            .Create();

            await BrokerageContext.ServiceUsers.AddAsync(serviceUser);
            await BrokerageContext.ServiceUsers.AddAsync(anotherServiceUser);
            await BrokerageContext.SaveChangesAsync();

            var request = Fixture.BuildServiceUserRequest(null)
            .With(sur => sur.ServiceUserName, "Fake Person")
            .Without(sur => sur.DateOfBirth)
            .Create();
            // Act
            var result = await _classUnderTest.GetByRequestAsync(request);

            // Assert
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result, Contains.Item(anotherServiceUser));
        }
        [Test]
        public async Task GetsExpectedServiceUsersByDoB()
        {
            // Arrange
            var serviceUser = Fixture.BuildServiceUser().Create();
            var anotherServiceUser = Fixture.BuildServiceUser()
            .Create();

            await BrokerageContext.ServiceUsers.AddAsync(serviceUser);
            await BrokerageContext.ServiceUsers.AddAsync(anotherServiceUser);
            await BrokerageContext.SaveChangesAsync();

            var request = Fixture.BuildServiceUserRequest(null)
            .Without(sur => sur.ServiceUserName)
            .With(sur => sur.DateOfBirth, anotherServiceUser.DateOfBirth)
            .Create();
            // Act
            var result = await _classUnderTest.GetByRequestAsync(request);

            // Assert
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result, Contains.Item(anotherServiceUser));
        }
        [Test]
        public async Task GetsExpectedServiceUsersByNameAndDoB()
        {
            // Arrange
            var serviceUser = Fixture.BuildServiceUser().Create();
            var anotherServiceUser = Fixture.BuildServiceUser()
            .Create();

            await BrokerageContext.ServiceUsers.AddAsync(serviceUser);
            await BrokerageContext.ServiceUsers.AddAsync(anotherServiceUser);
            await BrokerageContext.SaveChangesAsync();

            var request = Fixture.BuildServiceUserRequest(null)
            .With(sur => sur.ServiceUserName, anotherServiceUser.ServiceUserName)
            .With(sur => sur.DateOfBirth, anotherServiceUser.DateOfBirth)
            .Create();
            // Act
            var result = await _classUnderTest.GetByRequestAsync(request);

            // Assert
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result, Contains.Item(anotherServiceUser));
        }
        [Test]
        public async Task ReturnsEmptyListWhenNoServiceUsersMatch()
        {
            // Arrange
            var serviceUser = Fixture.BuildServiceUser().Create();
            var anotherServiceUser = Fixture.BuildServiceUser()
            .Create();

            await BrokerageContext.ServiceUsers.AddAsync(serviceUser);
            await BrokerageContext.ServiceUsers.AddAsync(anotherServiceUser);
            await BrokerageContext.SaveChangesAsync();

            var request = Fixture.BuildServiceUserRequest(null)
            .With(sur => sur.ServiceUserName, "Nonexistent User")
            .Create();
            // Act
            var result = await _classUnderTest.GetByRequestAsync(request);

            // Assert
            Assert.That(result, Has.Count.EqualTo(0));
            Assert.That(result, Does.Not.Contain(serviceUser));
            Assert.That(result, Does.Not.Contain(anotherServiceUser));




        }
    }
}
