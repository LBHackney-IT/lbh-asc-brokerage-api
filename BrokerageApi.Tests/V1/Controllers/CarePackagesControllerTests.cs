using AutoFixture;
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
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BrokerageApi.V1.UseCase.Interfaces;
using BrokerageApi.V1.UseCase.Interfaces.CarePackages;

namespace BrokerageApi.Tests.V1.Controllers
{
    [TestFixture]
    public class CarePackagesControllerTests : ControllerTests
    {
        private Fixture _fixture;
        private Mock<IGetCarePackageByIdUseCase> _mockGetCarePackageByIdUseCase;
        private Mock<IStartCarePackageUseCase> _mockStartCarePackageUseCase;

        private CarePackagesController _classUnderTest;
        private Mock<IEndCarePackageUseCase> _mockEndCarePackageUseCase;
        private Mock<ISuspendCarePackageUseCase> _mockSuspendCarePackageUseCase;
        private Mock<ICancelCarePackageUseCase> _mockCancelCarePackageUseCase;
        private Mock<IGetBudgetApproversUseCase> _mockGetBudgetApproversUseCase;
        private Mock<IAssignBudgetApproverToCarePackageUseCase> _mockAssignBudgetApproverUseCase;
        private Mock<IApproveCarePackageUseCase> _mockApproveCarePackageUseCase;
        private Mock<IRequestAmendmentToCarePackageUseCase> _mockRequestAmendmentUseCase;

        [SetUp]
        public void SetUp()
        {
            _fixture = FixtureHelpers.Fixture;
            _mockGetCarePackageByIdUseCase = new Mock<IGetCarePackageByIdUseCase>();
            _mockStartCarePackageUseCase = new Mock<IStartCarePackageUseCase>();
            _mockEndCarePackageUseCase = new Mock<IEndCarePackageUseCase>();
            _mockCancelCarePackageUseCase = new Mock<ICancelCarePackageUseCase>();
            _mockSuspendCarePackageUseCase = new Mock<ISuspendCarePackageUseCase>();
            _mockGetBudgetApproversUseCase = new Mock<IGetBudgetApproversUseCase>();
            _mockAssignBudgetApproverUseCase = new Mock<IAssignBudgetApproverToCarePackageUseCase>();
            _mockApproveCarePackageUseCase = new Mock<IApproveCarePackageUseCase>();
            _mockRequestAmendmentUseCase = new Mock<IRequestAmendmentToCarePackageUseCase>();

            _classUnderTest = new CarePackagesController(
                _mockGetCarePackageByIdUseCase.Object,
                _mockStartCarePackageUseCase.Object,
                _mockEndCarePackageUseCase.Object,
                _mockCancelCarePackageUseCase.Object,
                _mockSuspendCarePackageUseCase.Object,
                _mockGetBudgetApproversUseCase.Object,
                _mockAssignBudgetApproverUseCase.Object,
                _mockApproveCarePackageUseCase.Object,
                _mockRequestAmendmentUseCase.Object
            );

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
            var response = await _classUnderTest.GetCarePackage(carePackage.Id);
            var statusCode = GetStatusCode(response);
            var result = GetResultData<CarePackageResponse>(response);

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
            var response = await _classUnderTest.GetCarePackage(123456);
            var statusCode = GetStatusCode(response);
            var result = GetResultData<ProblemDetails>(response);

            // Assert
            statusCode.Should().Be((int) HttpStatusCode.NotFound);
            result.Status.Should().Be((int) HttpStatusCode.NotFound);
            result.Detail.Should().Be("Care package not found for: 123456 (Parameter 'id')");
        }

        [Test, Property("AsUser", "Broker")]
        public async Task StartCarePackage()
        {
            // Arrange
            var referral = _fixture.BuildReferral(ReferralStatus.Assigned)
                .With(x => x.AssignedBrokerEmail, "a.broker@hackney.gov.uk")
                .Create();

            _mockStartCarePackageUseCase
                .Setup(x => x.ExecuteAsync(referral.Id))
                .ReturnsAsync(referral);

            // Act
            var response = await _classUnderTest.StartCarePackage(referral.Id);
            var statusCode = GetStatusCode(response);
            var result = GetResultData<ReferralResponse>(response);

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
            var response = await _classUnderTest.StartCarePackage(123456);
            var statusCode = GetStatusCode(response);
            var result = GetResultData<ProblemDetails>(response);

            // Assert
            statusCode.Should().Be((int) HttpStatusCode.NotFound);
            result.Status.Should().Be((int) HttpStatusCode.NotFound);
            result.Detail.Should().Be("Referral not found for: 123456 (Parameter 'referralId')");
        }

        [Test, Property("AsUser", "Broker")]
        public async Task StartCarePackageWhenReferralIsUnassigned()
        {
            // Arrange
            var referral = _fixture.BuildReferral(ReferralStatus.Unassigned)
                .Create();

            _mockStartCarePackageUseCase
                .Setup(x => x.ExecuteAsync(referral.Id))
                .ThrowsAsync(new InvalidOperationException("Referral is not in a valid state to start editing"));

            // Act
            var response = await _classUnderTest.StartCarePackage(referral.Id);
            var statusCode = GetStatusCode(response);
            var result = GetResultData<ProblemDetails>(response);

            // Assert
            statusCode.Should().Be((int) HttpStatusCode.UnprocessableEntity);
            result.Status.Should().Be((int) HttpStatusCode.UnprocessableEntity);
            result.Detail.Should().Be("Referral is not in a valid state to start editing");
        }

        [Test, Property("AsUser", "Broker")]
        public async Task StartCarePackageWhenReferralIsAssignedToSomeoneElse()
        {
            // Arrange
            var referral = _fixture.BuildReferral(ReferralStatus.Assigned)
                .With(x => x.AssignedBrokerEmail, "other.broker@hackney.gov.uk")
                .Create();

            _mockStartCarePackageUseCase
                .Setup(x => x.ExecuteAsync(referral.Id))
                .ThrowsAsync(new UnauthorizedAccessException("Referral is not assigned to a.broker@hackney.gov.uk"));

            // Act
            var response = await _classUnderTest.StartCarePackage(referral.Id);
            var statusCode = GetStatusCode(response);
            var result = GetResultData<ProblemDetails>(response);

            // Assert
            statusCode.Should().Be((int) HttpStatusCode.Forbidden);
            result.Status.Should().Be((int) HttpStatusCode.Forbidden);
            result.Detail.Should().Be("Referral is not assigned to a.broker@hackney.gov.uk");
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
        public async Task CanSuspendCarePackage([Values] bool withEndDate)
        {
            const int referralId = 1234;
            var requestBuilder = _fixture.Build<SuspendRequest>()
                .With(r => r.Comment, "commentHere");

            if (!withEndDate)
            {
                requestBuilder = requestBuilder.Without(r => r.EndDate);
            }

            var request = requestBuilder.Create();

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
            var result = GetResultData<ProblemDetails>(response);

            statusCode.Should().Be((int) expectedStatusCode);
            result.Status.Should().Be((int) expectedStatusCode);
            result.Detail.Should().Be(exception.Message);
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
            var result = GetResultData<ProblemDetails>(response);

            statusCode.Should().Be((int) expectedStatusCode);
            result.Status.Should().Be((int) expectedStatusCode);
            result.Detail.Should().Be(exception.Message);
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
            var result = GetResultData<ProblemDetails>(response);

            statusCode.Should().Be((int) expectedStatusCode);
            result.Status.Should().Be((int) expectedStatusCode);
            result.Detail.Should().Be(exception.Message);
        }

        [Test]
        public async Task CanGetBudgetApprovers()
        {
            const int referralId = 1234;
            var expectedApprovers = _fixture.BuildUser().CreateMany();
            const decimal expectedEstimatedYearlyCost = 12345678;

            _mockGetBudgetApproversUseCase
                .Setup(x => x.ExecuteAsync(referralId))
                .ReturnsAsync((expectedApprovers, expectedEstimatedYearlyCost));

            var response = await _classUnderTest.GetBudgetApprovers(referralId);
            var statusCode = GetStatusCode(response);
            var result = GetResultData<GetApproversResponse>(response);

            statusCode.Should().Be((int) HttpStatusCode.OK);
            result.EstimatedYearlyCost.Should().Be(expectedEstimatedYearlyCost);
            result.Approvers.Should().BeEquivalentTo(expectedApprovers.Select(u => u.ToResponse()));
        }

        private static readonly object[] _getBudgetApproversErrorList =
        {
            new object[]
            {
                new ArgumentNullException(null, "message"), HttpStatusCode.NotFound
            },
            new object[]
            {
                new InvalidOperationException("message"), HttpStatusCode.UnprocessableEntity
            }
        };


        [TestCaseSource(nameof(_getBudgetApproversErrorList)), Property("AsUser", "Broker")]
        public async Task GetBudgetApproversMapsErrors(Exception exception, HttpStatusCode expectedStatusCode)
        {
            const int referralId = 1234;
            _mockGetBudgetApproversUseCase
                .Setup(x => x.ExecuteAsync(referralId))
                .ThrowsAsync(exception);

            var response = await _classUnderTest.GetBudgetApprovers(referralId);
            var statusCode = GetStatusCode(response);
            var result = GetResultData<ProblemDetails>(response);

            statusCode.Should().Be((int) expectedStatusCode);
            result.Status.Should().Be((int) expectedStatusCode);
            result.Detail.Should().Be(exception.Message);
        }

        [Test, Property("AsUser", "Broker")]
        public async Task CanAssignBudgetApprover()
        {
            const int referralId = 1234;
            var request = _fixture.Create<AssignApproverRequest>();

            var response = await _classUnderTest.AssignBudgetApprover(referralId, request);
            var statusCode = GetStatusCode(response);

            statusCode.Should().Be((int) HttpStatusCode.OK);
            _mockAssignBudgetApproverUseCase.Verify(x => x.ExecuteAsync(referralId, request.Approver));
        }

        private static readonly object[] _approverErrorList =
        {
            new object[]
            {
                new ArgumentNullException(null, "message"), HttpStatusCode.NotFound
            },
            new object[]
            {
                new UnauthorizedAccessException("message"), HttpStatusCode.Forbidden
            },
            new object[]
            {
                new InvalidOperationException("message"), HttpStatusCode.UnprocessableEntity
            }
        };


        [TestCaseSource(nameof(_approverErrorList)), Property("AsUser", "Broker")]
        public async Task AssignBudgetApproverMapsErrors(Exception exception, HttpStatusCode expectedStatusCode)
        {
            const int referralId = 1234;
            var request = _fixture.Create<AssignApproverRequest>();
            _mockAssignBudgetApproverUseCase
                .Setup(x => x.ExecuteAsync(referralId, request.Approver))
                .ThrowsAsync(exception);

            var response = await _classUnderTest.AssignBudgetApprover(referralId, request);
            var statusCode = GetStatusCode(response);
            var result = GetResultData<ProblemDetails>(response);

            statusCode.Should().Be((int) expectedStatusCode);
            result.Status.Should().Be((int) expectedStatusCode);
            result.Detail.Should().Be(exception.Message);
        }

        [Test, Property("AsUser", "Approver")]
        public async Task CanApproveCarePackage()
        {
            const int referralId = 1234;

            var response = await _classUnderTest.ApproveCarePackage(referralId);
            var statusCode = GetStatusCode(response);

            statusCode.Should().Be((int) HttpStatusCode.OK);
            _mockApproveCarePackageUseCase.Verify(x => x.ExecuteAsync(referralId));
        }

        [TestCaseSource(nameof(_approverErrorList)), Property("AsUser", "Approver")]
        public async Task CanApproveCarePackageMapsErrors(Exception exception, HttpStatusCode expectedStatusCode)
        {
            const int referralId = 1234;
            _mockApproveCarePackageUseCase
                .Setup(x => x.ExecuteAsync(referralId))
                .ThrowsAsync(exception);

            var response = await _classUnderTest.ApproveCarePackage(referralId);
            var statusCode = GetStatusCode(response);
            var result = GetResultData<ProblemDetails>(response);

            statusCode.Should().Be((int) expectedStatusCode);
            result.Status.Should().Be((int) expectedStatusCode);
            result.Detail.Should().Be(exception.Message);
        }

        [Test, Property("AsUser", "Approver")]
        public async Task CanRequestAmendment()
        {
            const int referralId = 1234;
            var request = _fixture.Create<AmendmentRequest>();

            var response = await _classUnderTest.RequestAmendment(referralId, request);
            var statusCode = GetStatusCode(response);

            statusCode.Should().Be((int) HttpStatusCode.OK);
            _mockRequestAmendmentUseCase.Verify(x => x.ExecuteAsync(referralId, request.Comment));
        }

        [TestCaseSource(nameof(_approverErrorList)), Property("AsUser", "Approver")]
        public async Task CanRequestAmendmentMapsErrors(Exception exception, HttpStatusCode expectedStatusCode)
        {
            const int referralId = 1234;
            var request = _fixture.Create<AmendmentRequest>();
            _mockRequestAmendmentUseCase
                .Setup(x => x.ExecuteAsync(referralId, It.IsAny<string>()))
                .ThrowsAsync(exception);

            var response = await _classUnderTest.RequestAmendment(referralId, request);
            var statusCode = GetStatusCode(response);
            var result = GetResultData<ProblemDetails>(response);

            statusCode.Should().Be((int) expectedStatusCode);
            result.Status.Should().Be((int) expectedStatusCode);
            result.Detail.Should().Be(exception.Message);
        }
    }
}
