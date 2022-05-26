using AutoFixture;
using BrokerageApi.Tests.V1.Controllers.Mocks;
using BrokerageApi.Tests.V1.Helpers;
using BrokerageApi.V1.Boundary.Request;
using BrokerageApi.V1.Boundary.Response;
using BrokerageApi.V1.Controllers;
using BrokerageApi.V1.Factories;
using BrokerageApi.V1.Infrastructure;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;
using System.Net;
using System.Threading.Tasks;
using BrokerageApi.V1.UseCase.Interfaces.CarePackages;

namespace BrokerageApi.Tests.V1.Controllers
{
    [TestFixture]
    public class CarePackagesControllerTests : ControllerTests
    {
        private Fixture _fixture;
        private Mock<IGetCarePackageByIdUseCase> _mockGetCarePackageByIdUseCase;
        private Mock<IStartCarePackageUseCase> _mockStartCarePackageUseCase;
        private MockProblemDetailsFactory _mockProblemDetailsFactory;

        private CarePackagesController _classUnderTest;
        private Mock<IEndCarePackageUseCase> _mockEndCarePackageUseCase;
        private Mock<ISuspendCarePackageUseCase> _mockSuspendCarePackageUseCase;
        private Mock<ICancelCarePackageUseCase> _mockCancelCarePackageUseCase;

        [SetUp]
        public void SetUp()
        {
            _fixture = FixtureHelpers.Fixture;
            _mockGetCarePackageByIdUseCase = new Mock<IGetCarePackageByIdUseCase>();
            _mockStartCarePackageUseCase = new Mock<IStartCarePackageUseCase>();
            _mockProblemDetailsFactory = new MockProblemDetailsFactory();
            _mockEndCarePackageUseCase = new Mock<IEndCarePackageUseCase>();
            _mockCancelCarePackageUseCase = new Mock<ICancelCarePackageUseCase>();
            _mockSuspendCarePackageUseCase = new Mock<ISuspendCarePackageUseCase>();

            _classUnderTest = new CarePackagesController(
                _mockGetCarePackageByIdUseCase.Object,
                _mockStartCarePackageUseCase.Object,
                _mockEndCarePackageUseCase.Object,
                _mockCancelCarePackageUseCase.Object,
                _mockSuspendCarePackageUseCase.Object
            );

            // .NET 3.1 doesn't set ProblemDetailsFactory so we need to mock it
            _classUnderTest.ProblemDetailsFactory = _mockProblemDetailsFactory.Object;

            SetupAuthentication(_classUnderTest);
        }

        [Test]
        public async Task GetCarePackage()
        {
            // Arrange
            var carePackage = _fixture.Create<CarePackage>();
            _mockGetCarePackageByIdUseCase
                .Setup(x => x.ExecuteAsync(carePackage.Id))
                .ReturnsAsync(carePackage);

            // Act
            var objectResult = await _classUnderTest.GetCarePackage(carePackage.Id);
            var statusCode = GetStatusCode(objectResult);
            var result = GetResultData<CarePackageResponse>(objectResult);

            // Assert
            statusCode.Should().Be((int) HttpStatusCode.OK);
            result.Should().BeEquivalentTo(carePackage.ToResponse());
        }

        [Test]
        public async Task GetCarePackageWhenDoesNotExist()
        {
            // Arrange
            _mockGetCarePackageByIdUseCase
                .Setup(x => x.ExecuteAsync(123456))
                .Callback((int id) => throw new ArgumentNullException(nameof(id), "Care package not found for: 123456"))
                .Returns(Task.FromResult(new CarePackage()));

            // Act
            var objectResult = await _classUnderTest.GetCarePackage(123456);
            var statusCode = GetStatusCode(objectResult);

            // Assert
            statusCode.Should().Be((int) HttpStatusCode.NotFound);
            _mockProblemDetailsFactory.VerifyProblem(HttpStatusCode.NotFound);
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
            _mockProblemDetailsFactory.VerifyProblem(HttpStatusCode.NotFound);
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
            _mockProblemDetailsFactory.VerifyProblem(HttpStatusCode.UnprocessableEntity);
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
            _mockProblemDetailsFactory.VerifyProblem(HttpStatusCode.Forbidden);
        }

        [Test, Property("AsUser", "Broker")]
        public async Task CanEndCarePackage()
        {
            const int referralId = 1234;
            var request = _fixture.Create<EndRequest>();

            var response = await _classUnderTest.EndCarePackage(referralId, request);
            var statusCode = GetStatusCode(response);

            statusCode.Should().Be((int) HttpStatusCode.OK);
            _mockEndCarePackageUseCase.Verify(x => x.ExecuteAsync(referralId, request.EndDate, request.Comment));
        }

        [Test, Property("AsUser", "Broker")]
        public async Task CanCancelCarePackage()
        {
            const int referralId = 1234;
            var request = _fixture.Create<CancelRequest>();

            var response = await _classUnderTest.CancelCarePackage(referralId, request);
            var statusCode = GetStatusCode(response);

            statusCode.Should().Be((int) HttpStatusCode.OK);
            _mockCancelCarePackageUseCase.Verify(x => x.ExecuteAsync(referralId, request.Comment));
        }

        [Test, Property("AsUser", "Broker")]
        public async Task CanSuspendCarePackage()
        {
            const int referralId = 1234;
            var request = _fixture.Create<SuspendRequest>();

            var response = await _classUnderTest.SuspendCarePackage(referralId, request);
            var statusCode = GetStatusCode(response);

            statusCode.Should().Be((int) HttpStatusCode.OK);
            _mockSuspendCarePackageUseCase.Verify(x => x.ExecuteAsync(referralId, request.StartDate, request.EndDate, request.Comment));
        }

        private static readonly object[] _errorList =
        {
            new object[]
            {
                new ArgumentNullException(null, "message"), HttpStatusCode.NotFound
            },
            new object[]
            {
                new ArgumentException("message"), HttpStatusCode.BadRequest
            },
            new object[]
            {
                new InvalidOperationException("message"), HttpStatusCode.UnprocessableEntity
            }
        };

        [TestCaseSource(nameof(_errorList)), Property("AsUser", "Broker")]
        public async Task EndCarePackageMapsErrors(Exception exception, HttpStatusCode expectedStatusCode)
        {
            const int referralId = 1234;
            var request = _fixture.Create<EndRequest>();
            _mockEndCarePackageUseCase.Setup(x => x.ExecuteAsync(referralId, request.EndDate, It.IsAny<string>()))
                .ThrowsAsync(exception);

            var response = await _classUnderTest.EndCarePackage(referralId, request);
            var statusCode = GetStatusCode(response);

            statusCode.Should().Be((int) expectedStatusCode);
            _mockProblemDetailsFactory.VerifyProblem(expectedStatusCode, exception.Message);
        }

        [TestCaseSource(nameof(_errorList)), Property("AsUser", "Broker")]
        public async Task CancelCarePackageMapsErrors(Exception exception, HttpStatusCode expectedStatusCode)
        {
            const int referralId = 1234;
            var request = _fixture.Create<CancelRequest>();
            _mockCancelCarePackageUseCase.Setup(x => x.ExecuteAsync(referralId, It.IsAny<string>()))
                .ThrowsAsync(exception);

            var response = await _classUnderTest.CancelCarePackage(referralId, request);
            var statusCode = GetStatusCode(response);

            statusCode.Should().Be((int) expectedStatusCode);
            _mockProblemDetailsFactory.VerifyProblem(expectedStatusCode, exception.Message);
        }

        [TestCaseSource(nameof(_errorList)), Property("AsUser", "Broker")]
        public async Task SuspendCarePackageMapsErrors(Exception exception, HttpStatusCode expectedStatusCode)
        {
            const int referralId = 1234;
            var request = _fixture.Create<SuspendRequest>();
            _mockSuspendCarePackageUseCase.Setup(x => x.ExecuteAsync(referralId, request.StartDate, request.EndDate, It.IsAny<string>()))
                .ThrowsAsync(exception);

            var response = await _classUnderTest.SuspendCarePackage(referralId, request);
            var statusCode = GetStatusCode(response);

            statusCode.Should().Be((int) expectedStatusCode);
            _mockProblemDetailsFactory.VerifyProblem(expectedStatusCode, exception.Message);
        }
    }
}
