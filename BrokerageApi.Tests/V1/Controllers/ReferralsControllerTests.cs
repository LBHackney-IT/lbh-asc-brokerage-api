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
using System.Net;
using System.Threading.Tasks;

namespace BrokerageApi.Tests.V1.Controllers
{
    [TestFixture]
    public class ReferralsControllerTests : ControllerTests
    {
        private Fixture _fixture;
        private Mock<ICreateReferralUseCase> _createReferralUseCaseMock;
        private MockProblemDetailsFactory _problemDetailsFactoryMock;

        private ReferralsController _classUnderTest;

        [SetUp]
        public void SetUp()
        {
            _fixture = FixtureHelpers.Fixture;
            _createReferralUseCaseMock = new Mock<ICreateReferralUseCase>();
            _problemDetailsFactoryMock = new MockProblemDetailsFactory();

            _classUnderTest = new ReferralsController(_createReferralUseCaseMock.Object);

            // .NET 3.1 doesn't set ProblemDetailsFactory so we need to mock it
            _classUnderTest.ProblemDetailsFactory = _problemDetailsFactoryMock.Object;
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
    }
}
