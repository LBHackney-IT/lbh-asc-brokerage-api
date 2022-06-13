using System.Linq;
using System.Net;
using AutoFixture;
using BrokerageApi.Tests.V1.Helpers;
using BrokerageApi.V1.Boundary.Response;
using BrokerageApi.V1.Controllers;
using BrokerageApi.V1.Factories;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.Infrastructure.AuditEvents;
using BrokerageApi.V1.UseCase.Interfaces;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using X.PagedList;

namespace BrokerageApi.Tests.V1.Controllers
{
    [TestFixture]
    public class AuditControllerTests : ControllerTests
    {
        private Mock<IGetServiceUserAuditEventsUseCase> _mockAuditUseCase;
        private AuditController _classUnderTest;
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _fixture = FixtureHelpers.Fixture;
            _mockAuditUseCase = new Mock<IGetServiceUserAuditEventsUseCase>();

            _classUnderTest = new AuditController(_mockAuditUseCase.Object);
        }

        [Test]
        public void CanGetServiceUserAuditTrail()
        {
            const string socialCareId = "testId";
            const int pageNumber = 1;
            const int pageSize = 10;

            var referral = new Referral()
            {
                Id = 1234,
                WorkflowId = "174079ae-75b4-43b4-9d29-363e88e7dd40",
                WorkflowType = WorkflowType.Assessment,
                FormName = "Care act assessment",
                SocialCareId = "33556688",
                ResidentName = "A Service User",
                PrimarySupportReason = "Physical Support",
                DirectPayments = "No",
                AssignedBrokerEmail = "a.broker@hackney.gov.uk",
                Status = ReferralStatus.Approved
            };

            var auditEvents = _fixture.BuildAuditEvent()
                .With(ae => ae.SocialCareId, socialCareId)
                .With(ae => ae.Metadata, "{ \"referralId\": 1234 }")
                .With(ae => ae.Referral, referral)
                .CreateMany(pageSize)
                .AsQueryable()
                .ToPagedList(pageNumber, pageSize);

            _mockAuditUseCase.Setup(x => x.Execute(socialCareId, pageNumber, pageSize))
                .Returns(auditEvents);

            var objectResult = _classUnderTest.GetAuditEvents(socialCareId, pageNumber, pageSize);
            var statusCode = GetStatusCode(objectResult);
            var result = GetResultData<GetServiceUserAuditEventsResponse>(objectResult);

            statusCode.Should().Be((int) HttpStatusCode.OK);
            var expectedResult = auditEvents.Select(ae => ae.ToResponse());
            result.Events.Should().BeEquivalentTo(expectedResult);
            result.PageMetadata.Should().BeEquivalentTo(auditEvents.GetMetaData().ToResponse());
        }
    }
}
