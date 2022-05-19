using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoFixture;
using BrokerageApi.Tests.V1.Helpers;
using BrokerageApi.V1.Boundary.Response;
using BrokerageApi.V1.Factories;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.Infrastructure.AuditEvents;
using FluentAssertions;
using NUnit.Framework;
using X.PagedList;

namespace BrokerageApi.Tests.V1.E2ETests
{
    public class AuditEventTests : IntegrationTests<Startup>
    {
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _fixture = FixtureHelpers.Fixture;
        }

        [Test, Property("AsUser", "Broker")]
        public async Task CanGetAuditEvents()
        {
            const string socialCareId = "testId";
            const int pageNumber = 1;
            const int pageSize = 10;

            var user = _fixture.Create<User>();

            var auditEvents = _fixture.Build<AuditEvent>()
                .With(ae => ae.UserId, user.Id)
                .With(ae => ae.SocialCareId, socialCareId)
                .Without(ae => ae.User)
                .With(ae => ae.Metadata, "{ \"test\": \"test\" }")
                .CreateMany(100);

            await Context.Users.AddAsync(user);
            await Context.AuditEvents.AddRangeAsync(auditEvents);
            await Context.SaveChangesAsync();

            var (code, response) = await Get<GetServiceUserAuditEventsResponse>($"/api/v1/serviceuser/{socialCareId}?pageNumber={pageNumber}&pageSize={pageSize}");

            code.Should().Be(HttpStatusCode.OK);
            var expectedEvents = auditEvents
                .OrderBy(ae => ae.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(ae => ae.ToResponse());
            response.Events.Should().BeEquivalentTo(expectedEvents);
            response.PageMetadata.Should().BeEquivalentTo(auditEvents.AsQueryable().ToPagedList(pageNumber, pageSize).GetMetaData().ToResponse());

            Context.ChangeTracker.Clear();
        }
    }
}