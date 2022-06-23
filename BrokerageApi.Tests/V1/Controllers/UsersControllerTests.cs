using System;
using AutoFixture;
using BrokerageApi.Tests.V1.Controllers.Mocks;
using BrokerageApi.Tests.V1.Helpers;
using BrokerageApi.V1.Boundary.Response;
using BrokerageApi.V1.Controllers;
using BrokerageApi.V1.Factories;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.UseCase.Interfaces;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace BrokerageApi.Tests.V1.Controllers
{
    [TestFixture]
    public class UsersControllerTests : ControllerTests
    {
        private Fixture _fixture;
        private Mock<IGetAllUsersUseCase> _mockGetAllUsersUseCase;
        private MockProblemDetailsFactory _mockProblemDetailsFactory;

        private UsersController _classUnderTest;
        private Mock<IGetCurrentUserUseCase> _mockCurrentUserUseCase;

        [SetUp]
        public void SetUp()
        {
            _fixture = FixtureHelpers.Fixture;
            _mockGetAllUsersUseCase = new Mock<IGetAllUsersUseCase>();
            _mockCurrentUserUseCase = new Mock<IGetCurrentUserUseCase>();
            _mockProblemDetailsFactory = new MockProblemDetailsFactory();

            _classUnderTest = new UsersController(
                _mockGetAllUsersUseCase.Object,
                _mockCurrentUserUseCase.Object
            );

            // .NET 3.1 doesn't set ProblemDetailsFactory so we need to mock it
            _classUnderTest.ProblemDetailsFactory = _mockProblemDetailsFactory.Object;
        }

        [Test]
        public async Task GetAllUsers()
        {
            // Arrange
            var users = _fixture.BuildUser().CreateMany();
            _mockGetAllUsersUseCase
                .Setup(x => x.ExecuteAsync(null))
                .ReturnsAsync(users);

            // Act
            var response = await _classUnderTest.GetAllUsers(null);
            var statusCode = GetStatusCode(response);
            var result = GetResultData<List<UserResponse>>(response);

            // Assert
            statusCode.Should().Be((int) HttpStatusCode.OK);
            result.Should().BeEquivalentTo(users.Select(u => u.ToResponse()).ToList());
        }

        [Test]
        public async Task GetFilteredAllUsers()
        {
            // Arrange
            var users = _fixture.BuildUser().CreateMany();
            _mockGetAllUsersUseCase
                .Setup(x => x.ExecuteAsync(UserRole.Broker))
                .ReturnsAsync(users);

            // Act
            var response = await _classUnderTest.GetAllUsers(UserRole.Broker);
            var statusCode = GetStatusCode(response);
            var result = GetResultData<List<UserResponse>>(response);

            // Assert
            statusCode.Should().Be((int) HttpStatusCode.OK);
            result.Should().BeEquivalentTo(users.Select(u => u.ToResponse()).ToList());
        }

        [Test]
        public async Task GetCurrentUser()
        {
            // Arrange
            var user = _fixture.BuildUser().Create();
            _mockCurrentUserUseCase
                .Setup(x => x.ExecuteAsync())
                .ReturnsAsync(user);

            // Act
            var response = await _classUnderTest.GetCurrentUser();
            var statusCode = GetStatusCode(response);
            var result = GetResultData<UserResponse>(response);

            // Assert
            statusCode.Should().Be((int) HttpStatusCode.OK);
            result.Should().BeEquivalentTo(user.ToResponse());
        }

        [Test]
        public async Task Rerturns404WhenUserNotFound()
        {
            // Arrange
            const string expectedMessage = "message";
            _mockCurrentUserUseCase
                .Setup(x => x.ExecuteAsync())
                .ThrowsAsync(new ArgumentException(expectedMessage));

            // Act
            var response = await _classUnderTest.GetCurrentUser();
            var statusCode = GetStatusCode(response);

            // Assert
            statusCode.Should().Be((int) HttpStatusCode.NotFound);
            _mockProblemDetailsFactory.VerifyProblem(HttpStatusCode.NotFound, expectedMessage);
        }
    }
}
