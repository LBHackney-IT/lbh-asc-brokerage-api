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
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace BrokerageApi.Tests.V1.Controllers
{
    [TestFixture]
    public class ReferralsControllerTests : ControllerTests
    {
        private Fixture _fixture;
        private Mock<ICreateReferralUseCase> _createReferralUseCaseMock;
        private Mock<IGetAssignedReferralsUseCase> _getAssignedReferralsUseCaseMock;
        private Mock<IGetCurrentReferralsUseCase> _getCurrentReferralsUseCaseMock;
        private Mock<IGetReferralByIdUseCase> _getReferralByIdUseCaseMock;
        private Mock<IAssignBrokerToReferralUseCase> _assignBrokerToReferralUseCaseMock;
        private Mock<IReassignBrokerToReferralUseCase> _reassignBrokerToReferralUseCaseMock;
        private MockProblemDetailsFactory _problemDetailsFactoryMock;

        private ReferralsController _classUnderTest;

        [SetUp]
        public void SetUp()
        {
            _fixture = FixtureHelpers.Fixture;
            _createReferralUseCaseMock = new Mock<ICreateReferralUseCase>();
            _getAssignedReferralsUseCaseMock = new Mock<IGetAssignedReferralsUseCase>();
            _getCurrentReferralsUseCaseMock = new Mock<IGetCurrentReferralsUseCase>();
            _getReferralByIdUseCaseMock = new Mock<IGetReferralByIdUseCase>();
            _assignBrokerToReferralUseCaseMock = new Mock<IAssignBrokerToReferralUseCase>();
            _reassignBrokerToReferralUseCaseMock = new Mock<IReassignBrokerToReferralUseCase>();
            _problemDetailsFactoryMock = new MockProblemDetailsFactory();

            _classUnderTest = new ReferralsController(
                _createReferralUseCaseMock.Object,
                _getAssignedReferralsUseCaseMock.Object,
                _getCurrentReferralsUseCaseMock.Object,
                _getReferralByIdUseCaseMock.Object,
                _assignBrokerToReferralUseCaseMock.Object,
                _reassignBrokerToReferralUseCaseMock.Object
            );

            // .NET 3.1 doesn't set ProblemDetailsFactory so we need to mock it
            _classUnderTest.ProblemDetailsFactory = _problemDetailsFactoryMock.Object;

            SetupAuthentication(_classUnderTest);
        }

        [Test]
        public async Task CreateReferral()
        {
            // Arrange
            var request = _fixture.Create<CreateReferralRequest>();
            var referral = _fixture.Create<Referral>();

            _createReferralUseCaseMock
                .Setup(x => x.ExecuteAsync(request))
                .ReturnsAsync(referral);

            // Act
            var objectResult = await _classUnderTest.CreateReferral(request);
            var statusCode = GetStatusCode(objectResult);
            var result = GetResultData<ReferralResponse>(objectResult);

            // Assert
            statusCode.Should().Be((int) HttpStatusCode.OK);
            result.Should().BeEquivalentTo(referral.ToResponse());
        }

        [Test]
        public async Task GetCurrentReferrals()
        {
            // Arrange
            var referrals = _fixture.CreateMany<Referral>();
            _getCurrentReferralsUseCaseMock.Setup(x => x.ExecuteAsync(null))
                .ReturnsAsync(referrals);

            // Act
            var objectResult = await _classUnderTest.GetCurrentReferrals(null);
            var statusCode = GetStatusCode(objectResult);
            var result = GetResultData<List<ReferralResponse>>(objectResult);

            // Assert
            statusCode.Should().Be((int) HttpStatusCode.OK);
            result.Should().BeEquivalentTo(referrals.Select(r => r.ToResponse()).ToList());
        }

        [Test]
        public async Task GetFilteredCurrentReferrals()
        {
            // Arrange
            var referrals = _fixture.CreateMany<Referral>();
            _getCurrentReferralsUseCaseMock.Setup(x => x.ExecuteAsync(ReferralStatus.Unassigned))
                .ReturnsAsync(referrals);

            // Act
            var objectResult = await _classUnderTest.GetCurrentReferrals(ReferralStatus.Unassigned);
            var statusCode = GetStatusCode(objectResult);
            var result = GetResultData<List<ReferralResponse>>(objectResult);

            // Assert
            statusCode.Should().Be((int) HttpStatusCode.OK);
            result.Should().BeEquivalentTo(referrals.Select(r => r.ToResponse()).ToList());
        }

        [Test, Property("AsUser", "Broker")]
        public async Task GetAssignedReferrals()
        {
            // Arrange
            var referrals = _fixture.CreateMany<Referral>();
            _getAssignedReferralsUseCaseMock.Setup(x => x.ExecuteAsync(null))
                .ReturnsAsync(referrals);

            // Act
            var objectResult = await _classUnderTest.GetAssignedReferrals(null);
            var statusCode = GetStatusCode(objectResult);
            var result = GetResultData<List<ReferralResponse>>(objectResult);

            // Assert
            statusCode.Should().Be((int) HttpStatusCode.OK);
            result.Should().BeEquivalentTo(referrals.Select(r => r.ToResponse()).ToList());
        }

        [Test, Property("AsUser", "Broker")]
        public async Task GetFilteredAssignedReferrals()
        {
            // Arrange
            var referrals = _fixture.CreateMany<Referral>();
            _getAssignedReferralsUseCaseMock.Setup(x => x.ExecuteAsync(ReferralStatus.InProgress))
                .ReturnsAsync(referrals);

            // Act
            var objectResult = await _classUnderTest.GetAssignedReferrals(ReferralStatus.InProgress);
            var statusCode = GetStatusCode(objectResult);
            var result = GetResultData<List<ReferralResponse>>(objectResult);

            // Assert
            statusCode.Should().Be((int) HttpStatusCode.OK);
            result.Should().BeEquivalentTo(referrals.Select(r => r.ToResponse()).ToList());
        }

        [Test]
        public async Task GetReferral()
        {
            // Arrange
            var referral = _fixture.Create<Referral>();
            _getReferralByIdUseCaseMock.Setup(x => x.ExecuteAsync(referral.Id))
                .ReturnsAsync(referral);

            // Act
            var objectResult = await _classUnderTest.GetReferral(referral.Id);
            var statusCode = GetStatusCode(objectResult);
            var result = GetResultData<ReferralResponse>(objectResult);

            // Assert
            statusCode.Should().Be((int) HttpStatusCode.OK);
            result.Should().BeEquivalentTo(referral.ToResponse());
        }

        [Test]
        public async Task GetReferralWhenDoesNotExist()
        {
            // Arrange
            _getReferralByIdUseCaseMock.Setup(x => x.ExecuteAsync(404))
                .ReturnsAsync(null as Referral);

            // Act
            var objectResult = await _classUnderTest.GetReferral(404);
            var statusCode = GetStatusCode(objectResult);

            // Assert
            statusCode.Should().Be((int) HttpStatusCode.NotFound);
            _problemDetailsFactoryMock.VerifyStatusCode(HttpStatusCode.NotFound);
        }

        [Test]
        public async Task AssignBroker()
        {
            // Arrange
            var request = _fixture.Build<AssignBrokerRequest>()
                .With(x => x.Broker, "a.broker@hackney.gov.uk")
                .Create();

            var referral = _fixture.Build<Referral>()
                .With(x => x.Status, ReferralStatus.Assigned)
                .Create();

            _assignBrokerToReferralUseCaseMock.Setup(x => x.ExecuteAsync(referral.Id, request))
                .ReturnsAsync(referral);

            // Act
            var objectResult = await _classUnderTest.AssignBroker(referral.Id, request);
            var statusCode = GetStatusCode(objectResult);
            var result = GetResultData<ReferralResponse>(objectResult);

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

            _assignBrokerToReferralUseCaseMock.Setup(x => x.ExecuteAsync(123456, request))
                .Callback((int referralId, AssignBrokerRequest request) => throw new ArgumentNullException(nameof(referralId), "Referral not found for: 123456"))
                .Returns(Task.FromResult(new Referral()));

            // Act
            var objectResult = await _classUnderTest.AssignBroker(123456, request);
            var statusCode = GetStatusCode(objectResult);

            // Assert
            statusCode.Should().Be((int) HttpStatusCode.NotFound);
            _problemDetailsFactoryMock.VerifyStatusCode(HttpStatusCode.NotFound);
        }

        [Test]
        public async Task AssignBrokerWhenReferralIsNotUnassigned()
        {
            // Arrange
            var request = _fixture.Build<AssignBrokerRequest>()
                .With(x => x.Broker, "a.broker@hackney.gov.uk")
                .Create();

            var referral = _fixture.Build<Referral>()
                .With(x => x.Status, ReferralStatus.Assigned)
                .Create();

            _assignBrokerToReferralUseCaseMock.Setup(x => x.ExecuteAsync(referral.Id, request))
                .ThrowsAsync(new InvalidOperationException("Referral is not in a valid state for assignment"));

            // Act
            var objectResult = await _classUnderTest.AssignBroker(referral.Id, request);
            var statusCode = GetStatusCode(objectResult);

            // Assert
            statusCode.Should().Be((int) HttpStatusCode.UnprocessableEntity);
            _problemDetailsFactoryMock.VerifyStatusCode(HttpStatusCode.UnprocessableEntity);
        }

        [Test]
        public async Task ReassignBroker()
        {
            // Arrange
            var request = _fixture.Build<AssignBrokerRequest>()
                .With(x => x.Broker, "a.broker@hackney.gov.uk")
                .Create();

            var referral = _fixture.Build<Referral>()
                .With(x => x.Status, ReferralStatus.Assigned)
                .Create();

            _reassignBrokerToReferralUseCaseMock.Setup(x => x.ExecuteAsync(referral.Id, request))
                .ReturnsAsync(referral);

            // Act
            var objectResult = await _classUnderTest.ReassignBroker(referral.Id, request);
            var statusCode = GetStatusCode(objectResult);
            var result = GetResultData<ReferralResponse>(objectResult);

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

            _reassignBrokerToReferralUseCaseMock.Setup(x => x.ExecuteAsync(123456, request))
                .Callback((int referralId, AssignBrokerRequest request) => throw new ArgumentNullException(nameof(referralId), "Referral not found for: 123456"))
                .Returns(Task.FromResult(new Referral()));

            // Act
            var objectResult = await _classUnderTest.ReassignBroker(123456, request);
            var statusCode = GetStatusCode(objectResult);

            // Assert
            statusCode.Should().Be((int) HttpStatusCode.NotFound);
            _problemDetailsFactoryMock.VerifyStatusCode(HttpStatusCode.NotFound);
        }

        [Test]
        public async Task ReassignBrokerWhenReferralIsUnassigned()
        {
            // Arrange
            var request = _fixture.Build<AssignBrokerRequest>()
                .With(x => x.Broker, "a.broker@hackney.gov.uk")
                .Create();

            var referral = _fixture.Build<Referral>()
                .With(x => x.Status, ReferralStatus.Unassigned)
                .Create();

            _reassignBrokerToReferralUseCaseMock.Setup(x => x.ExecuteAsync(referral.Id, request))
                .ThrowsAsync(new InvalidOperationException("Referral is not in a valid state for assignment"));

            // Act
            var objectResult = await _classUnderTest.ReassignBroker(referral.Id, request);
            var statusCode = GetStatusCode(objectResult);

            // Assert
            statusCode.Should().Be((int) HttpStatusCode.UnprocessableEntity);
            _problemDetailsFactoryMock.VerifyStatusCode(HttpStatusCode.UnprocessableEntity);
        }
    }
}
