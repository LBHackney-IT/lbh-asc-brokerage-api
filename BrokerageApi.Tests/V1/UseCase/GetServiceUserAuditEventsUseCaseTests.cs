using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using BrokerageApi.Tests.V1.Helpers;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure.AuditEvents;
using BrokerageApi.V1.UseCase;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using PagedList.Core;

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
            const string SocialCareId = "socialCareId";
            var expectedEvents = _fixture.CreateMany<AuditEvent>().AsQueryable().ToPagedList(1, 100);
            _auditGatewayMock.Setup(x => x.GetServiceUserAuditEvents(SocialCareId, It.IsAny<int>(), It.IsAny<int>()))
                .Returns(expectedEvents);

            var events = _classUnderTest.Execute(SocialCareId, 1, 100);

            events.Should().BeEquivalentTo(expectedEvents);
        }
    }
}
