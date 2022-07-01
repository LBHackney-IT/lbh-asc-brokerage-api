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
using BrokerageApi.V1.UseCase.Interfaces.CarePackageCareCharges;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BrokerageApi.Tests.V1.Controllers
{
    [TestFixture]
    public class CarePackageCareChargesControllerTests : ControllerTests
    {
        private Fixture _fixture;
        private Mock<ICreateCareChargeUseCase> _mockCreateCareChargeUseCase;
        private Mock<IDeleteCareChargeUseCase> _mockDeleteCareChargeUseCase;

        private CarePackageCareChargesController _classUnderTest;

        [SetUp]
        public void SetUp()
        {
            _fixture = FixtureHelpers.Fixture;
            _mockCreateCareChargeUseCase = new Mock<ICreateCareChargeUseCase>();
            _mockDeleteCareChargeUseCase = new Mock<IDeleteCareChargeUseCase>();

            _classUnderTest = new CarePackageCareChargesController(_mockCreateCareChargeUseCase.Object,
                _mockDeleteCareChargeUseCase.Object);

            SetupAuthentication(_classUnderTest);
        }

        [Test, Property("AsUser", "CareChargesOfficer")]
        public async Task CreatesCareCharge()
        {
            // Arrange
            var referral = _fixture.BuildReferral(ReferralStatus.Approved)
                .Create();

            var request = _fixture.Create<CreateCareChargeRequest>();

            _mockCreateCareChargeUseCase
                .Setup(x => x.ExecuteAsync(referral.Id, request))
                .ReturnsAsync(request.ToDatabase());

            // Act
            var response = await _classUnderTest.CreateCareCharge(referral.Id, request);
            var statusCode = GetStatusCode(response);
            var result = GetResultData<ElementResponse>(response);

            // Assert
            statusCode.Should().Be((int) HttpStatusCode.OK);
            result.Should().BeEquivalentTo(request.ToDatabase().ToResponse());
        }

        private static readonly object[] _createOrEditCareChargeErrors =
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
            }
        };

        [TestCaseSource(nameof(_createOrEditCareChargeErrors)), Property("AsUser", "CareChargesOfficer")]
        public async Task CreateCareChargeMapsErrors(Exception exception, HttpStatusCode expectedStatusCode)
        {
            // Arrange
            var referral = _fixture.BuildReferral(ReferralStatus.Assigned)
                .Create();

            var request = _fixture.Create<CreateCareChargeRequest>();

            _mockCreateCareChargeUseCase
                .Setup(x => x.ExecuteAsync(referral.Id, request))
                .Throws(exception);

            // Act
            var response = await _classUnderTest.CreateCareCharge(referral.Id, request);
            var statusCode = GetStatusCode(response);
            var result = GetResultData<ProblemDetails>(response);

            // Assert
            statusCode.Should().Be((int) expectedStatusCode);
            result.Status.Should().Be((int) expectedStatusCode);
            result.Detail.Should().Be(exception.Message);
        }

        public async Task DeletesCareCharge()
        {
            // Arrange
            var elements = _fixture.BuildElement(1, 1).CreateMany();
            var referral = _fixture.BuildReferral(ReferralStatus.Approved)
                .With(x => x.Elements, elements.ToList)
                .Create();
            var elementId = referral.Elements.First().Id;

            // Act
            var response = await _classUnderTest.DeleteCareCharge(referral.Id, elementId);
            var statusCode = GetStatusCode(response);

            // Assert
            _mockDeleteCareChargeUseCase.Verify(x => x.ExecuteAsync(referral.Id, elementId));
            statusCode.Should().Be((int) HttpStatusCode.OK);
        }

        private static readonly object[] _deleteCareChargeErrors =
        {
            new object[]
            {
                new ArgumentNullException(null, "message"), StatusCodes.Status404NotFound
            },
            new object[]
            {
                new InvalidOperationException("message"), StatusCodes.Status422UnprocessableEntity
            }
        };

        [TestCaseSource(nameof(_deleteCareChargeErrors)), Property("AsUser", "CareChargesOfficer")]
        public async Task DeleteCareChargeMapsErrors(Exception exception, HttpStatusCode expectedStatusCode)
        {
            // Arrange
            var referral = _fixture.BuildReferral(ReferralStatus.Approved)
                .Create();

            _mockDeleteCareChargeUseCase
                .Setup(x => x.ExecuteAsync(referral.Id, It.IsAny<int>()))
                .Throws(exception);

            // Act
            var response = await _classUnderTest.DeleteCareCharge(referral.Id, 1);
            var statusCode = GetStatusCode(response);
            var result = GetResultData<ProblemDetails>(response);

            // Assert
            statusCode.Should().Be((int) expectedStatusCode);
            result.Status.Should().Be((int) expectedStatusCode);
            result.Detail.Should().Be(exception.Message);
        }
    }
}
