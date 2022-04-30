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
    public class CarePackagesControllerTests : ControllerTests
    {
        private Fixture _fixture;
        private Mock<IStartCarePackageUseCase> _mockStartCarePackageUseCase;
        private Mock<ICreateElementUseCase> _mockCreateElementUseCase;
        private MockProblemDetailsFactory _mockProblemDetailsFactory;

        private CarePackagesController _classUnderTest;

        [SetUp]
        public void SetUp()
        {
            _fixture = FixtureHelpers.Fixture;
            _mockStartCarePackageUseCase = new Mock<IStartCarePackageUseCase>();
            _mockCreateElementUseCase = new Mock<ICreateElementUseCase>();
            _mockProblemDetailsFactory = new MockProblemDetailsFactory();

            _classUnderTest = new CarePackagesController(
                _mockStartCarePackageUseCase.Object,
                _mockCreateElementUseCase.Object
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
    }
}
