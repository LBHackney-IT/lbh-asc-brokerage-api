using System;
using System.Threading.Tasks;
using AutoFixture;
using BrokerageApi.Tests.V1.Helpers;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.Services.Interfaces;
using BrokerageApi.V1.UseCase;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace BrokerageApi.Tests.V1.UseCase
{

    public class GetCurrentUserUseCaseTests
    {
        private Mock<IUserGateway> _mockUserGateway;
        private GetCurrentUserUseCase _classUnderTest;
        private Fixture _fixture;
        private Mock<IUserService> _mockUserService;

        [SetUp]
        public void Setup()
        {
            _fixture = FixtureHelpers.Fixture;
            _mockUserService = new Mock<IUserService>();
            _mockUserGateway = new Mock<IUserGateway>();
            _classUnderTest = new GetCurrentUserUseCase(
                _mockUserService.Object,
                _mockUserGateway.Object
            );
        }

        [Test]
        public async Task CanGetCurrentUser()
        {
            var user = _fixture.BuildUser().Create();

            _mockUserService.Setup(x => x.Email)
                .Returns(user.Email);
            _mockUserGateway.Setup(x => x.GetByEmailAsync(user.Email))
                .ReturnsAsync(user);

            var result = await _classUnderTest.ExecuteAsync();

            result.Should().BeEquivalentTo(user);
        }

        [Test]
        public async Task ThrowsArgumentNullExceptionWhenUserDoesntExist()
        {
            const string expectedEmail = "someone@somwhere.com";

            _mockUserService.Setup(x => x.Email)
                .Returns(expectedEmail);
            _mockUserGateway.Setup(x => x.GetByEmailAsync(expectedEmail))
                .ReturnsAsync((User) null);


            Func<Task> act = () => _classUnderTest.ExecuteAsync();

            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage($"User not found for: {expectedEmail}");
        }
    }
}
