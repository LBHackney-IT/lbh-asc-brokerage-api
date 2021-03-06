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
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using BrokerageApi.V1.UseCase.Interfaces.CarePackageElements;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BrokerageApi.Tests.V1.Controllers
{
    [TestFixture]
    public class CarePackageElementsControllerTests : ControllerTests
    {
        private Fixture _fixture;
        private Mock<ICreateElementUseCase> _mockCreateElementUseCase;
        private Mock<IDeleteElementUseCase> _mockDeleteElementUseCase;
        private Mock<IEndElementUseCase> _mockEndElementUseCase;
        private Mock<ISuspendElementUseCase> _mockSuspendElementUseCase;
        private Mock<ICancelElementUseCase> _mockCancelElementUseCase;
        private Mock<IEditElementUseCase> _mockEditElementUseCase;
        private Mock<IResetElementUseCase> _mockResetElementUseCase;

        private CarePackageElementsController _classUnderTest;

        [SetUp]
        public void SetUp()
        {
            _fixture = FixtureHelpers.Fixture;
            _mockCreateElementUseCase = new Mock<ICreateElementUseCase>();
            _mockDeleteElementUseCase = new Mock<IDeleteElementUseCase>();
            _mockEndElementUseCase = new Mock<IEndElementUseCase>();
            _mockSuspendElementUseCase = new Mock<ISuspendElementUseCase>();
            _mockCancelElementUseCase = new Mock<ICancelElementUseCase>();
            _mockEditElementUseCase = new Mock<IEditElementUseCase>();
            _mockResetElementUseCase = new Mock<IResetElementUseCase>();

            _classUnderTest = new CarePackageElementsController(_mockCreateElementUseCase.Object,
                _mockDeleteElementUseCase.Object,
                _mockEndElementUseCase.Object,
                _mockCancelElementUseCase.Object,
                _mockSuspendElementUseCase.Object,
                _mockEditElementUseCase.Object,
                _mockResetElementUseCase.Object
            );

            SetupAuthentication(_classUnderTest);
        }

        [Test, Property("AsUser", "Broker")]
        public async Task CreatesElement()
        {
            // Arrange
            var referral = _fixture.BuildReferral(ReferralStatus.Assigned)
                .With(x => x.AssignedBrokerEmail, "a.broker@hackney.gov.uk")
                .Create();

            var request = _fixture.Create<CreateElementRequest>();

            _mockCreateElementUseCase
                .Setup(x => x.ExecuteAsync(referral.Id, request))
                .ReturnsAsync(request.ToDatabase());

            // Act
            var response = await _classUnderTest.CreateElement(referral.Id, request);
            var statusCode = GetStatusCode(response);
            var result = GetResultData<ElementResponse>(response);

            // Assert
            statusCode.Should().Be((int) HttpStatusCode.OK);
            result.Should().BeEquivalentTo(request.ToDatabase().ToResponse());
        }

        private static readonly object[] _createOrEditElementErrors =
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

        [TestCaseSource(nameof(_createOrEditElementErrors)), Property("AsUser", "Broker")]
        public async Task CreateElementMapsErrors(Exception exception, HttpStatusCode expectedStatusCode)
        {
            // Arrange
            var referral = _fixture.BuildReferral(ReferralStatus.Assigned)
                .With(x => x.AssignedBrokerEmail, "a.broker@hackney.gov.uk")
                .Create();

            var request = _fixture.Create<CreateElementRequest>();

            _mockCreateElementUseCase
                .Setup(x => x.ExecuteAsync(referral.Id, request))
                .Throws(exception);

            // Act
            var response = await _classUnderTest.CreateElement(referral.Id, request);
            var statusCode = GetStatusCode(response);
            var result = GetResultData<ProblemDetails>(response);

            // Assert
            statusCode.Should().Be((int) expectedStatusCode);
            result.Status.Should().Be((int) expectedStatusCode);
            result.Detail.Should().Be(exception.Message);
        }

        [Test, Property("AsUser", "Broker")]
        public async Task DeletesElement()
        {
            // Arrange
            var elements = _fixture.BuildElement(1, 1).CreateMany();
            var referral = _fixture.BuildReferral(ReferralStatus.Assigned)
                .With(x => x.AssignedBrokerEmail, "a.broker@hackney.gov.uk")
                .With(x => x.Elements, elements.ToList)
                .Create();
            var elementId = referral.Elements.First().Id;

            // Act
            var response = await _classUnderTest.DeleteElement(referral.Id, elementId);
            var statusCode = GetStatusCode(response);

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
        public async Task DeleteElementMapsErrors(Exception exception, HttpStatusCode expectedStatusCode)
        {
            // Arrange
            var referral = _fixture.BuildReferral(ReferralStatus.Assigned)
                .With(x => x.AssignedBrokerEmail, "a.broker@hackney.gov.uk")
                .Create();

            _mockDeleteElementUseCase
                .Setup(x => x.ExecuteAsync(referral.Id, It.IsAny<int>()))
                .Throws(exception);

            // Act
            var response = await _classUnderTest.DeleteElement(referral.Id, 1);
            var statusCode = GetStatusCode(response);
            var result = GetResultData<ProblemDetails>(response);

            // Assert
            statusCode.Should().Be((int) expectedStatusCode);
            result.Status.Should().Be((int) expectedStatusCode);
            result.Detail.Should().Be(exception.Message);
        }

        [Test]
        public async Task EndsElement()
        {
            const int referralId = 1234;
            const int elementId = 1234;
            var request = _fixture.Create<EndRequest>();

            var response = await _classUnderTest.EndElement(referralId, elementId, request);
            var statusCode = GetStatusCode(response);

            _mockEndElementUseCase.Verify(x => x.ExecuteAsync(referralId, elementId, request.EndDate));
            statusCode.Should().Be((int) HttpStatusCode.OK);
        }

        private static readonly object[] _endElementErrors =
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

        [TestCaseSource(nameof(_endElementErrors)), Property("AsUser", "Broker")]
        public async Task EndElementMapsErrors(Exception exception, HttpStatusCode expectedStatusCode)
        {
            const int referralId = 1234;
            const int elementId = 1234;
            var request = _fixture.Create<EndRequest>();
            _mockEndElementUseCase.Setup(x => x.ExecuteAsync(referralId, elementId, request.EndDate))
                .ThrowsAsync(exception);

            var response = await _classUnderTest.EndElement(referralId, elementId, request);
            var statusCode = GetStatusCode(response);
            var result = GetResultData<ProblemDetails>(response);

            statusCode.Should().Be((int) expectedStatusCode);
            result.Status.Should().Be((int) expectedStatusCode);
            result.Detail.Should().Be(exception.Message);
        }

        [Test]
        public async Task CancelsElement()
        {
            const int referralId = 1234;
            const int elementId = 1234;
            var request = _fixture.Create<CancelRequest>();

            var response = await _classUnderTest.CancelElement(referralId, elementId, request);
            var statusCode = GetStatusCode(response);

            _mockCancelElementUseCase.Verify(x => x.ExecuteAsync(referralId, elementId, request.Comment));
            statusCode.Should().Be((int) HttpStatusCode.OK);
        }

        private static readonly object[] _cancelElementErrors =
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

        [TestCaseSource(nameof(_cancelElementErrors)), Property("AsUser", "Broker")]
        public async Task CancelElementMapsErrors(Exception exception, HttpStatusCode expectedStatusCode)
        {
            const int referralId = 1234;
            const int elementId = 1234;
            var request = _fixture.Create<CancelRequest>();
            _mockCancelElementUseCase.Setup(x => x.ExecuteAsync(referralId, elementId, It.IsAny<string>()))
                .ThrowsAsync(exception);

            var response = await _classUnderTest.CancelElement(referralId, elementId, request);
            var statusCode = GetStatusCode(response);
            var result = GetResultData<ProblemDetails>(response);

            statusCode.Should().Be((int) expectedStatusCode);
            result.Status.Should().Be((int) expectedStatusCode);
            result.Detail.Should().Be(exception.Message);
        }

        [Test, Property("AsUser", "Broker")]
        public async Task EditsElement()
        {
            // Arrange
            var element = _fixture.BuildElement(1, 1)
                .Create();

            var referral = _fixture.BuildReferral(ReferralStatus.Assigned)
                .With(x => x.AssignedBrokerEmail, "a.broker@hackney.gov.uk")
                .With(x => x.Elements, new List<Element> { element })
                .Create();

            var request = _fixture.Create<EditElementRequest>();

            _mockEditElementUseCase
                .Setup(x => x.ExecuteAsync(referral.Id, element.Id, request))
                .ReturnsAsync(request.ToDatabase(element));

            // Act
            var response = await _classUnderTest.EditElement(referral.Id, element.Id, request);
            var statusCode = GetStatusCode(response);
            var result = GetResultData<ElementResponse>(response);

            // Assert
            statusCode.Should().Be((int) HttpStatusCode.OK);
            result.Should().BeEquivalentTo(request.ToDatabase(element).ToResponse());
        }

        [TestCaseSource(nameof(_createOrEditElementErrors)), Property("AsUser", "Broker")]
        public async Task EditElementMapsErrors(Exception exception, HttpStatusCode expectedStatusCode)
        {
            // Arrange
            var element = _fixture.BuildElement(1, 1)
                .Create();

            var referral = _fixture.BuildReferral(ReferralStatus.Assigned)
                .With(x => x.AssignedBrokerEmail, "a.broker@hackney.gov.uk")
                .With(x => x.Elements, new List<Element> { element })
                .Create();

            var request = _fixture.Create<EditElementRequest>();

            _mockEditElementUseCase
                .Setup(x => x.ExecuteAsync(referral.Id, element.Id, request))
                .Throws(exception);

            // Act
            var response = await _classUnderTest.EditElement(referral.Id, element.Id, request);
            var statusCode = GetStatusCode(response);
            var result = GetResultData<ProblemDetails>(response);

            // Assert
            statusCode.Should().Be((int) expectedStatusCode);
            result.Status.Should().Be((int) expectedStatusCode);
            result.Detail.Should().Be(exception.Message);
        }

        [Test]
        public async Task SuspendsElement([Values] bool withEndDate)
        {
            const int referralId = 1234;
            const int elementId = 1234;
            var requestBuilder = _fixture.Build<SuspendRequest>()
                .With(r => r.Comment, "commentHere");

            if (!withEndDate)
            {
                requestBuilder = requestBuilder.Without(r => r.EndDate);
            }

            var request = requestBuilder.Create();

            var response = await _classUnderTest.SuspendElement(referralId, elementId, request);
            var statusCode = GetStatusCode(response);

            _mockSuspendElementUseCase.Verify(x => x.ExecuteAsync(referralId, elementId, request.StartDate, request.EndDate, request.Comment));
            statusCode.Should().Be((int) HttpStatusCode.OK);
        }

        [TestCaseSource(nameof(_endElementErrors)), Property("AsUser", "Broker")]
        public async Task SuspendElementMapsErrors(Exception exception, HttpStatusCode expectedStatusCode)
        {
            const int referralId = 1234;
            const int elementId = 1234;
            var request = _fixture.Create<SuspendRequest>();
            _mockSuspendElementUseCase.Setup(x => x.ExecuteAsync(referralId, elementId, request.StartDate, request.EndDate, It.IsAny<string>()))
                .ThrowsAsync(exception);

            var response = await _classUnderTest.SuspendElement(referralId, elementId, request);
            var statusCode = GetStatusCode(response);
            var result = GetResultData<ProblemDetails>(response);

            statusCode.Should().Be((int) expectedStatusCode);
            result.Status.Should().Be((int) expectedStatusCode);
            result.Detail.Should().Be(exception.Message);
        }

        [Test]
        public async Task ResetsElement()
        {
            const int referralId = 1234;
            const int elementId = 1234;

            var response = await _classUnderTest.ResetElement(referralId, elementId);
            var statusCode = GetStatusCode(response);

            _mockResetElementUseCase.Verify(x => x.ExecuteAsync(referralId, elementId));
            statusCode.Should().Be((int) HttpStatusCode.OK);
        }

        private static readonly object[] _resetElementErrors =
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


        [TestCaseSource(nameof(_resetElementErrors)), Property("AsUser", "Broker")]
        public async Task ResetElementMapsErrors(Exception exception, HttpStatusCode expectedStatusCode)
        {
            const int referralId = 1234;
            const int elementId = 1234;

            _mockResetElementUseCase.Setup(x => x.ExecuteAsync(referralId, elementId))
                .ThrowsAsync(exception);

            var response = await _classUnderTest.ResetElement(referralId, elementId);
            var statusCode = GetStatusCode(response);
            var result = GetResultData<ProblemDetails>(response);

            statusCode.Should().Be((int) expectedStatusCode);
            result.Status.Should().Be((int) expectedStatusCode);
            result.Detail.Should().Be(exception.Message);
        }
    }
}
