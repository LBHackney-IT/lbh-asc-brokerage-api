using AutoFixture;
using System;
using System.Threading.Tasks;
using BrokerageApi.Tests.V1.Helpers;
using BrokerageApi.V1.Gateways;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.UseCase;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace BrokerageApi.Tests.V1.UseCase
{
    public class GetUserByEmailUseCaseTests
    {
        private Mock<IUserGateway> _mockUserGateway;
        private GetUserByEmailUseCase _classUnderTest;
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _fixture = FixtureHelpers.Fixture;
            _mockUserGateway = new Mock<IUserGateway>();
            _classUnderTest = new GetUserByEmailUseCase(_mockUserGateway.Object);
        }

        [Test]
        public async Task GetUser()
        {
            // Arrange
            var user = _fixture.Create<User>();
            _mockUserGateway
                .Setup(x => x.GetByEmailAsync(user.Email))
                .ReturnsAsync(user);

            // Act
            var result = await _classUnderTest.ExecuteAsync(user.Email);

            // Assert
            result.Should().BeEquivalentTo(user);
        }
    }
}
