using AutoFixture;
using BrokerageApi.Tests.V1.Controllers.Mocks;
using BrokerageApi.Tests.V1.Helpers;
using BrokerageApi.V1.Boundary.Request;
using BrokerageApi.V1.Boundary.Response;
using BrokerageApi.V1.Controllers;
using BrokerageApi.V1.Factories;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.UseCase.Interfaces;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace BrokerageApi.Tests.V1.Controllers
{
    [TestFixture]
    public class CarePackagesControllerTests : ControllerTests
    {
        private Fixture _fixture;
        private Mock<IStartCarePackageUseCase> _mockStartCarePackageUseCase;
        private Mock<ICreateElementUseCase> _mockCreateElementUseCase;
        private MockProblemDetailsFactory _mockProblemDetailsFactory;
        private Mock<IDeleteElementUseCase> _mockDeleteElementUseCase;

        private CarePackagesController _classUnderTest;

        [SetUp]
        public void SetUp()
        {
            _fixture = FixtureHelpers.Fixture;
            _mockStartCarePackageUseCase = new Mock<IStartCarePackageUseCase>();
            _mockCreateElementUseCase = new Mock<ICreateElementUseCase>();
            _mockProblemDetailsFactory = new MockProblemDetailsFactory();
            _mockDeleteElementUseCase = new Mock<IDeleteElementUseCase>();

            _classUnderTest = new CarePackagesController(
                _mockStartCarePackageUseCase.Object,
                _mockCreateElementUseCase.Object,
                _mockDeleteElementUseCase.Object
            );

            // .NET 3.1 doesn't set ProblemDetailsFactory so we need to mock it
            _classUnderTest.ProblemDetailsFactory = _mockProblemDetailsFactory.Object;

            SetupAuthentication(_classUnderTest);
        }

        [Test, Property("AsUser", "Broker")]
        public async Task StartCarePackage()
        {
            // Arrange
            var referral = _fixture.Build<Referral>()
                .With(x => x.Status, ReferralStatus.Assigned)
                .With(x => x.AssignedTo, "a.broker@hackney.gov.uk")
                .Create();

            _mockStartCarePackageUseCase
                .Setup(x => x.ExecuteAsync(referral.Id))
                .ReturnsAsync(referral);

            // Act
            var objectResult = await _classUnderTest.StartCarePackage(referral.Id);
            var statusCode = GetStatusCode(objectResult);
            var result = GetResultData<ReferralResponse>(objectResult);

            // Assert
            statusCode.Should().Be((int) HttpStatusCode.OK);
            result.Should().BeEquivalentTo(referral.ToResponse());
        }

        [Test, Property("AsUser", "Broker")]
        public async Task StartCarePackageWhenReferralDoesNotExist()
        {
            // Arrange
            _mockStartCarePackageUseCase
                .Setup(x => x.ExecuteAsync(123456))
                .Callback((int referralId) => throw new ArgumentNullException(nameof(referralId), "Referral not found for: 123456"))
                .Returns(Task.FromResult(new Referral()));

            // Act
            var objectResult = await _classUnderTest.StartCarePackage(123456);
            var statusCode = GetStatusCode(objectResult);

            // Assert
            statusCode.Should().Be((int) HttpStatusCode.NotFound);
            _mockProblemDetailsFactory.VerifyStatusCode(HttpStatusCode.NotFound);
        }

        [Test, Property("AsUser", "Broker")]
        public async Task StartCarePackageWhenReferralIsUnassigned()
        {
            // Arrange
            var referral = _fixture.Build<Referral>()
                .With(x => x.Status, ReferralStatus.Unassigned)
                .Create();

            _mockStartCarePackageUseCase
                .Setup(x => x.ExecuteAsync(referral.Id))
                .ThrowsAsync(new InvalidOperationException("Referral is not in a valid state to start editing"));

            // Act
            var objectResult = await _classUnderTest.StartCarePackage(referral.Id);
            var statusCode = GetStatusCode(objectResult);

            // Assert
            statusCode.Should().Be((int) HttpStatusCode.UnprocessableEntity);
            _mockProblemDetailsFactory.VerifyStatusCode(HttpStatusCode.UnprocessableEntity);
        }

        [Test, Property("AsUser", "Broker")]
        public async Task StartCarePackageWhenReferralIsAssignedToSomeoneElse()
        {
            // Arrange
            var referral = _fixture.Build<Referral>()
                .With(x => x.Status, ReferralStatus.Assigned)
                .With(x => x.AssignedTo, "other.broker@hackney.gov.uk")
                .Create();

            _mockStartCarePackageUseCase
                .Setup(x => x.ExecuteAsync(referral.Id))
                .ThrowsAsync(new UnauthorizedAccessException("Referral is not assigned to a.broker@hackney.gov.uk"));

            // Act
            var objectResult = await _classUnderTest.StartCarePackage(referral.Id);
            var statusCode = GetStatusCode(objectResult);

            // Assert
            statusCode.Should().Be((int) HttpStatusCode.Forbidden);
            _mockProblemDetailsFactory.VerifyStatusCode(HttpStatusCode.Forbidden);
        }

        [Test, Property("AsUser", "Broker")]
        public async Task CreatesElement()
        {
            // Arrange
            var referral = _fixture.Build<Referral>()
                .With(x => x.Status, ReferralStatus.Assigned)
                .With(x => x.AssignedTo, "a.broker@hackney.gov.uk")
                .Create();

            var request = _fixture.Create<CreateElementRequest>();

            _mockCreateElementUseCase
                .Setup(x => x.ExecuteAsync(referral.Id, request))
                .ReturnsAsync(request.ToDatabase());

            // Act
            var objectResult = await _classUnderTest.CreateElement(referral.Id, request);
            var statusCode = GetStatusCode(objectResult);
            var result = GetResultData<ElementResponse>(objectResult);

            // Assert
            statusCode.Should().Be((int) HttpStatusCode.OK);
            result.Should().BeEquivalentTo(request.ToDatabase().ToResponse());
        }

        private static readonly object[] _createElementErrors =
        {
            new object[]
            {
                new ArgumentNullException(null, "message"), StatusCodes.Status404NotFound
            },
            new object[]
            {
                new ArgumentException("message"), StatusCodes.Status400BadRequest
            },
            new object[]
            {
                new InvalidOperationException("message"), StatusCodes.Status422UnprocessableEntity
            },
            new object[]
            {
                new UnauthorizedAccessException("message"), StatusCodes.Status403Forbidden
            }
        };

        [TestCaseSource(nameof(_createElementErrors)), Property("AsUser", "Broker")]
        public async Task CreateElementMapsErrors(Exception exception, int expectedStatusCode)
        {
            // Arrange
            var referral = _fixture.Build<Referral>()
                .With(x => x.Status, ReferralStatus.Assigned)
                .With(x => x.AssignedTo, "a.broker@hackney.gov.uk")
                .Create();

            var request = _fixture.Create<CreateElementRequest>();

            _mockCreateElementUseCase
                .Setup(x => x.ExecuteAsync(referral.Id, request))
                .Throws(exception);

            // Act
            var objectResult = await _classUnderTest.CreateElement(referral.Id, request);
            var statusCode = GetStatusCode(objectResult);

            // Assert
            statusCode.Should().Be(expectedStatusCode);
        }

        [Test, Property("AsUser", "Broker")]
        public async Task DeletesElement()
        {
            // Arrange
            var referral = _fixture.Build<Referral>()
                .With(x => x.Status, ReferralStatus.Assigned)
                .With(x => x.AssignedTo, "a.broker@hackney.gov.uk")
                .Create();
            var elementId = referral.Elements.First().Id;

            // Act
            var objectResult = await _classUnderTest.DeleteElement(referral.Id, elementId);
            var statusCode = GetStatusCode(objectResult);

            // Assert
            _mockDeleteElementUseCase.Verify(x => x.ExecuteAsync(referral.Id, elementId));
            statusCode.Should().Be((int) HttpStatusCode.OK);
        }

        private static readonly object[] _deleteElementErrors =
        {
            new object[]
            {
                new ArgumentNullException(null, "message"), StatusCodes.Status404NotFound
            },
            new object[]
            {
                new InvalidOperationException("message"), StatusCodes.Status422UnprocessableEntity
            },
            new object[]
            {
                new UnauthorizedAccessException("message"), StatusCodes.Status403Forbidden
            }
        };

        [TestCaseSource(nameof(_deleteElementErrors)), Property("AsUser", "Broker")]
        public async Task DeleteElementMapsErrors(Exception exception, int expectedStatusCode)
        {
            // Arrange
            var referral = _fixture.Build<Referral>()
                .With(x => x.Status, ReferralStatus.Assigned)
                .With(x => x.AssignedTo, "a.broker@hackney.gov.uk")
                .Create();

            _mockDeleteElementUseCase
                .Setup(x => x.ExecuteAsync(referral.Id, It.IsAny<int>()))
                .Throws(exception);

            // Act
            var objectResult = await _classUnderTest.DeleteElement(referral.Id, referral.Elements.First().Id);
            var statusCode = GetStatusCode(objectResult);

            // Assert
            statusCode.Should().Be(expectedStatusCode);
        }
    }
}
