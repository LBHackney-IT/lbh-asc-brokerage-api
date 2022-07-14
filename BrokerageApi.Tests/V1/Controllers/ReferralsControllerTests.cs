using AutoFixture;
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
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace BrokerageApi.Tests.V1.Controllers
{

    [TestFixture]
    public class ReferralsControllerTests : ControllerTests
    {
        private Fixture _fixture;
        private Mock<ICreateReferralUseCase> _mockCreateReferralUseCase;
        private Mock<IGetApprovedReferralsUseCase> _mockGetApprovedReferralsUseCase;
        private Mock<IGetAssignedReferralsUseCase> _mockGetAssignedReferralsUseCase;
        private Mock<IGetCurrentReferralsUseCase> _mockGetCurrentReferralsUseCase;
        private Mock<IGetReferralByIdUseCase> _mockGetReferralByIdUseCase;
        private Mock<IAssignBrokerToReferralUseCase> _mockAssignBrokerToReferralUseCase;
        private Mock<IReassignBrokerToReferralUseCase> _mockReassignBrokerToReferralUseCase;
        private Mock<IGetBudgetApprovalsUseCase> _mockGetBudgetApprovalsUseCase;

        private ReferralsController _classUnderTest;
        private Mock<IArchiveReferralUseCase> _mockArchiveReferralUseCase;

        [SetUp]
        public void SetUp()
        {
            _fixture = FixtureHelpers.Fixture;
            _mockCreateReferralUseCase = new Mock<ICreateReferralUseCase>();
            _mockGetApprovedReferralsUseCase = new Mock<IGetApprovedReferralsUseCase>();
            _mockGetAssignedReferralsUseCase = new Mock<IGetAssignedReferralsUseCase>();
            _mockGetCurrentReferralsUseCase = new Mock<IGetCurrentReferralsUseCase>();
            _mockGetReferralByIdUseCase = new Mock<IGetReferralByIdUseCase>();
            _mockAssignBrokerToReferralUseCase = new Mock<IAssignBrokerToReferralUseCase>();
            _mockReassignBrokerToReferralUseCase = new Mock<IReassignBrokerToReferralUseCase>();
            _mockArchiveReferralUseCase = new Mock<IArchiveReferralUseCase>();
            _mockGetBudgetApprovalsUseCase = new Mock<IGetBudgetApprovalsUseCase>();

            _classUnderTest = new ReferralsController(
                _mockCreateReferralUseCase.Object,
                _mockGetApprovedReferralsUseCase.Object,
                _mockGetAssignedReferralsUseCase.Object,
                _mockGetCurrentReferralsUseCase.Object,
                _mockGetReferralByIdUseCase.Object,
                _mockAssignBrokerToReferralUseCase.Object,
                _mockReassignBrokerToReferralUseCase.Object,
                _mockArchiveReferralUseCase.Object,
                _mockGetBudgetApprovalsUseCase.Object
            );

            SetupAuthentication(_classUnderTest);
        }

        [Test]
        public async Task CreateReferral()
        {
            // Arrange
            var request = _fixture.Create<CreateReferralRequest>();
            var referral = _fixture.BuildReferral().Create();

            _mockCreateReferralUseCase
                .Setup(x => x.ExecuteAsync(request))
                .ReturnsAsync(referral);

            // Act
            var response = await _classUnderTest.CreateReferral(request);
            var statusCode = GetStatusCode(response);
            var result = GetResultData<ReferralResponse>(response);

            // Assert
            statusCode.Should().Be((int) HttpStatusCode.OK);
            result.Should().BeEquivalentTo(referral.ToResponse());
        }

        [Test]
        public async Task CreateReferralHandlesInvalidOperation()
        {
            // Arrange
            const string expectedMessage = "message";
            var request = _fixture.Create<CreateReferralRequest>();

            _mockCreateReferralUseCase
                .Setup(x => x.ExecuteAsync(request))
                .ThrowsAsync(new InvalidOperationException(expectedMessage));

            // Act
            var response = await _classUnderTest.CreateReferral(request);
            var statusCode = GetStatusCode(response);
            var result = GetResultData<ProblemDetails>(response);

            // Assert
            statusCode.Should().Be((int) HttpStatusCode.UnprocessableEntity);
            result.Status.Should().Be((int) HttpStatusCode.UnprocessableEntity);
            result.Detail.Should().Be(expectedMessage);
        }

        [Test]
        public async Task GetCurrentReferrals()
        {
            // Arrange
            var referrals = _fixture.BuildReferral().CreateMany();
            _mockGetCurrentReferralsUseCase
                .Setup(x => x.ExecuteAsync(null))
                .ReturnsAsync(referrals);

            // Act
            var response = await _classUnderTest.GetCurrentReferrals(null);
            var statusCode = GetStatusCode(response);
            var result = GetResultData<List<ReferralResponse>>(response);

            // Assert
            statusCode.Should().Be((int) HttpStatusCode.OK);
            result.Should().BeEquivalentTo(referrals.Select(r => r.ToResponse()).ToList());
        }

        [Test]
        public async Task GetFilteredCurrentReferrals()
        {
            // Arrange
            var referrals = _fixture.BuildReferral().CreateMany();
            _mockGetCurrentReferralsUseCase
                .Setup(x => x.ExecuteAsync(ReferralStatus.Unassigned))
                .ReturnsAsync(referrals);

            // Act
            var response = await _classUnderTest.GetCurrentReferrals(ReferralStatus.Unassigned);
            var statusCode = GetStatusCode(response);
            var result = GetResultData<List<ReferralResponse>>(response);

            // Assert
            statusCode.Should().Be((int) HttpStatusCode.OK);
            result.Should().BeEquivalentTo(referrals.Select(r => r.ToResponse()).ToList());
        }

        [Test, Property("AsUser", "CareChargesOfficer")]
        public async Task GetApprovedReferrals()
        {
            // Arrange
            var referrals = _fixture.BuildReferral().CreateMany();
            _mockGetApprovedReferralsUseCase
                .Setup(x => x.ExecuteAsync())
                .ReturnsAsync(referrals);

            // Act
            var response = await _classUnderTest.GetApprovedReferrals();
            var statusCode = GetStatusCode(response);
            var result = GetResultData<List<ReferralResponse>>(response);

            // Assert
            statusCode.Should().Be((int) HttpStatusCode.OK);
            result.Should().BeEquivalentTo(referrals.Select(r => r.ToResponse()).ToList());
        }

        [Test, Property("AsUser", "Broker")]
        public async Task GetAssignedReferrals()
        {
            // Arrange
            var referrals = _fixture.BuildReferral().CreateMany();
            _mockGetAssignedReferralsUseCase
                .Setup(x => x.ExecuteAsync(null))
                .ReturnsAsync(referrals);

            // Act
            var response = await _classUnderTest.GetAssignedReferrals(null);
            var statusCode = GetStatusCode(response);
            var result = GetResultData<List<ReferralResponse>>(response);

            // Assert
            statusCode.Should().Be((int) HttpStatusCode.OK);
            result.Should().BeEquivalentTo(referrals.Select(r => r.ToResponse()).ToList());
        }

        [Test, Property("AsUser", "Broker")]
        public async Task GetFilteredAssignedReferrals()
        {
            // Arrange
            var referrals = _fixture.BuildReferral().CreateMany();
            _mockGetAssignedReferralsUseCase
                .Setup(x => x.ExecuteAsync(ReferralStatus.InProgress))
                .ReturnsAsync(referrals);

            // Act
            var response = await _classUnderTest.GetAssignedReferrals(ReferralStatus.InProgress);
            var statusCode = GetStatusCode(response);
            var result = GetResultData<List<ReferralResponse>>(response);

            // Assert
            statusCode.Should().Be((int) HttpStatusCode.OK);
            result.Should().BeEquivalentTo(referrals.Select(r => r.ToResponse()).ToList());
        }

        [Test]
        public async Task GetReferral()
        {
            // Arrange
            var referral = _fixture.BuildReferral().Create();
            _mockGetReferralByIdUseCase
                .Setup(x => x.ExecuteAsync(referral.Id))
                .ReturnsAsync(referral);

            // Act
            var response = await _classUnderTest.GetReferral(referral.Id);
            var statusCode = GetStatusCode(response);
            var result = GetResultData<ReferralResponse>(response);

            // Assert
            statusCode.Should().Be((int) HttpStatusCode.OK);
            result.Should().BeEquivalentTo(referral.ToResponse());
        }

        [Test]
        public async Task GetReferralWhenDoesNotExist()
        {
            // Arrange
            _mockGetReferralByIdUseCase
                .Setup(x => x.ExecuteAsync(123456))
                .Callback((int id) => throw new ArgumentNullException(nameof(id), "Referral not found for: 123456"))
                .Returns(Task.FromResult(new Referral()));

            // Act
            var response = await _classUnderTest.GetReferral(123456);
            var statusCode = GetStatusCode(response);
            var result = GetResultData<ProblemDetails>(response);

            // Assert
            statusCode.Should().Be((int) HttpStatusCode.NotFound);
            result.Status.Should().Be((int) HttpStatusCode.NotFound);
            result.Detail.Should().Be("Referral not found for: 123456 (Parameter 'id')");
        }

        [Test]
        public async Task AssignBroker()
        {
            // Arrange
            var request = _fixture.Build<AssignBrokerRequest>()
                .With(x => x.Broker, "a.broker@hackney.gov.uk")
                .Create();

            var referral = _fixture.BuildReferral(ReferralStatus.Assigned)
                .Create();

            _mockAssignBrokerToReferralUseCase
                .Setup(x => x.ExecuteAsync(referral.Id, request))
                .ReturnsAsync(referral);

            // Act
            var response = await _classUnderTest.AssignBroker(referral.Id, request);
            var statusCode = GetStatusCode(response);
            var result = GetResultData<ReferralResponse>(response);

            // Assert
            statusCode.Should().Be((int) HttpStatusCode.OK);
            result.Should().BeEquivalentTo(referral.ToResponse());
        }

        [Test]
        public async Task AssignBrokerWhenReferralDoesNotExist()
        {
            // Arrange
            var request = _fixture.Build<AssignBrokerRequest>()
                .With(x => x.Broker, "a.broker@hackney.gov.uk")
                .Create();

            _mockAssignBrokerToReferralUseCase
                .Setup(x => x.ExecuteAsync(123456, request))
                .Callback((int referralId, AssignBrokerRequest request) => throw new ArgumentNullException(nameof(referralId), "Referral not found for: 123456"))
                .Returns(Task.FromResult(new Referral()));

            // Act
            var response = await _classUnderTest.AssignBroker(123456, request);
            var statusCode = GetStatusCode(response);
            var result = GetResultData<ProblemDetails>(response);

            // Assert
            statusCode.Should().Be((int) HttpStatusCode.NotFound);
            result.Status.Should().Be((int) HttpStatusCode.NotFound);
            result.Detail.Should().Be("Referral not found for: 123456 (Parameter 'referralId')");
        }

        [Test]
        public async Task AssignBrokerWhenReferralIsNotUnassigned()
        {
            // Arrange
            var request = _fixture.Build<AssignBrokerRequest>()
                .With(x => x.Broker, "a.broker@hackney.gov.uk")
                .Create();

            var referral = _fixture.BuildReferral(ReferralStatus.Assigned)
                .Create();

            _mockAssignBrokerToReferralUseCase
                .Setup(x => x.ExecuteAsync(referral.Id, request))
                .ThrowsAsync(new InvalidOperationException("Referral is not in a valid state for assignment"));

            // Act
            var response = await _classUnderTest.AssignBroker(referral.Id, request);
            var statusCode = GetStatusCode(response);
            var result = GetResultData<ProblemDetails>(response);

            // Assert
            statusCode.Should().Be((int) HttpStatusCode.UnprocessableEntity);
            result.Status.Should().Be((int) HttpStatusCode.UnprocessableEntity);
            result.Detail.Should().Be("Referral is not in a valid state for assignment");
        }

        [Test]
        public async Task ReassignBroker()
        {
            // Arrange
            var request = _fixture.Build<AssignBrokerRequest>()
                .With(x => x.Broker, "a.broker@hackney.gov.uk")
                .Create();

            var referral = _fixture.BuildReferral(ReferralStatus.Assigned)
                .Create();

            _mockReassignBrokerToReferralUseCase
                .Setup(x => x.ExecuteAsync(referral.Id, request))
                .ReturnsAsync(referral);

            // Act
            var response = await _classUnderTest.ReassignBroker(referral.Id, request);
            var statusCode = GetStatusCode(response);
            var result = GetResultData<ReferralResponse>(response);

            // Assert
            statusCode.Should().Be((int) HttpStatusCode.OK);
            result.Should().BeEquivalentTo(referral.ToResponse());
        }

        [Test]
        public async Task ReassignBrokerWhenReferralDoesNotExist()
        {
            // Arrange
            var request = _fixture.Build<AssignBrokerRequest>()
                .With(x => x.Broker, "a.broker@hackney.gov.uk")
                .Create();

            _mockReassignBrokerToReferralUseCase
                .Setup(x => x.ExecuteAsync(123456, request))
                .Callback((int referralId, AssignBrokerRequest request) => throw new ArgumentNullException(nameof(referralId), "Referral not found for: 123456"))
                .Returns(Task.FromResult(new Referral()));

            // Act
            var response = await _classUnderTest.ReassignBroker(123456, request);
            var statusCode = GetStatusCode(response);
            var result = GetResultData<ProblemDetails>(response);

            // Assert
            statusCode.Should().Be((int) HttpStatusCode.NotFound);
            result.Status.Should().Be((int) HttpStatusCode.NotFound);
            result.Detail.Should().Be("Referral not found for: 123456 (Parameter 'referralId')");
        }

        [Test]
        public async Task ReassignBrokerWhenReferralIsUnassigned()
        {
            // Arrange
            var request = _fixture.Build<AssignBrokerRequest>()
                .With(x => x.Broker, "a.broker@hackney.gov.uk")
                .Create();

            var referral = _fixture.BuildReferral(ReferralStatus.Unassigned)
                .Create();

            _mockReassignBrokerToReferralUseCase
                .Setup(x => x.ExecuteAsync(referral.Id, request))
                .ThrowsAsync(new InvalidOperationException("Referral is not in a valid state for assignment"));

            // Act
            var response = await _classUnderTest.ReassignBroker(referral.Id, request);
            var statusCode = GetStatusCode(response);
            var result = GetResultData<ProblemDetails>(response);

            // Assert
            statusCode.Should().Be((int) HttpStatusCode.UnprocessableEntity);
            result.Status.Should().Be((int) HttpStatusCode.UnprocessableEntity);
            result.Detail.Should().Be("Referral is not in a valid state for assignment");
        }

        [Test]
        public async Task ArchiveReferral()
        {
            // Arrange
            const int referralId = 1234;
            var request = _fixture.Build<ArchiveReferralRequest>()
                .Create();

            // Act
            var response = await _classUnderTest.ArchiveReferral(referralId, request);
            var statusCode = GetStatusCode(response);

            // Assert
            statusCode.Should().Be((int) HttpStatusCode.OK);
            _mockArchiveReferralUseCase.Verify(x => x.ExecuteAsync(referralId, request.Comment));
        }

        private static readonly object[] _archiveErrorList =
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

        [TestCaseSource(nameof(_archiveErrorList))]
        public async Task ArchiveReferralMapsErrorsCorrectly(Exception exception, HttpStatusCode expectedStatusCode)
        {
            // Arrange
            const int referralId = 1234;
            var request = _fixture.Build<ArchiveReferralRequest>()
                .Create();
            _mockArchiveReferralUseCase.Setup(x => x.ExecuteAsync(referralId, It.IsAny<string>()))
                .ThrowsAsync(exception);

            // Act
            var response = await _classUnderTest.ArchiveReferral(referralId, request);
            var statusCode = GetStatusCode(response);
            var result = GetResultData<ProblemDetails>(response);

            // Assert
            statusCode.Should().Be((int) expectedStatusCode);
            result.Status.Should().Be((int) expectedStatusCode);
            result.Detail.Should().Be(exception.Message);
        }

        [Test, Property("AsUser", "Approver")]
        public async Task GetBudgetApprovals()
        {
            // Arrange
            var carePackages = _fixture.BuildCarePackage().CreateMany();
            _mockGetBudgetApprovalsUseCase
                .Setup(x => x.ExecuteAsync())
                .ReturnsAsync(carePackages);

            // Act
            var response = await _classUnderTest.GetBudgetApprovals();
            var statusCode = GetStatusCode(response);
            var result = GetResultData<List<CarePackageResponse>>(response);

            // Assert
            statusCode.Should().Be((int) HttpStatusCode.OK);
            result.Should().BeEquivalentTo(carePackages.Select(c => c.ToResponse()).ToList());
        }

        private static readonly object[] _getBudgetApprovalsErrorList =
        {
            new object[]
            {
                new UnauthorizedAccessException("message"), HttpStatusCode.Unauthorized
            }
        };

        [TestCaseSource(nameof(_getBudgetApprovalsErrorList))]
        public async Task GetBudgetApprovalsMapsErrorsCorrectly(Exception exception, HttpStatusCode expectedStatusCode)
        {
            // Arrange
            _mockGetBudgetApprovalsUseCase
                .Setup(x => x.ExecuteAsync())
                .ThrowsAsync(exception);

            // Act
            var response = await _classUnderTest.GetBudgetApprovals();
            var statusCode = GetStatusCode(response);
            var result = GetResultData<ProblemDetails>(response);

            // Assert
            statusCode.Should().Be((int) expectedStatusCode);
            result.Status.Should().Be((int) expectedStatusCode);
            result.Detail.Should().Be(exception.Message);
        }
    }
}
