using System.Linq;
using AutoFixture;
using BrokerageApi.Tests.V1.Helpers;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.UseCase;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using X.PagedList;

namespace BrokerageApi.Tests.V1.UseCase
{
    public class GetServiceUserAuditEventsUseCaseTests
    {
        private Mock<IAuditGateway> _auditGatewayMock;
        private GetServiceUserAuditEventsUseCase _classUnderTest;
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _fixture = FixtureHelpers.Fixture;
            _auditGatewayMock = new Mock<IAuditGateway>();
            _classUnderTest = new GetServiceUserAuditEventsUseCase(_auditGatewayMock.Object);
        }

        [Test]
        public void CanGetEvents()
        {
            const string socialCareId = "socialCareId";
            var expectedEvents = _fixture.BuildAuditEvent().CreateMany().AsQueryable().ToPagedList(1, 100);
            _auditGatewayMock.Setup(x => x.GetServiceUserAuditEvents(socialCareId, It.IsAny<int>(), It.IsAny<int>()))
                .Returns(expectedEvents);

            var events = _classUnderTest.Execute(socialCareId, 1, 100);

            events.Should().BeEquivalentTo(expectedEvents);
        }
    }
}
