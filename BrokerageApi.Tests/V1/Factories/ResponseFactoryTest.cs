using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Dsl;
using AutoFixture.Kernel;
using BrokerageApi.Tests.V1.Helpers;
using BrokerageApi.V1.Factories;
using BrokerageApi.V1.Infrastructure.AuditEvents;
using FluentAssertions;
using Microsoft.VisualBasic;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using PagedList.Core;

namespace BrokerageApi.Tests.V1.Factories
{
    [TestFixture]
    public class ResponseFactoryTest
    {
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _fixture = FixtureHelpers.Fixture;
        }

        [Test]
        public void AuditResponseMapsCorrectly()
        {
            var auditEvent = _fixture.Build<AuditEvent>()
                .With(ae => ae.Metadata, "{ \"test\": \"test\" }")
                .Create();

            var response = auditEvent.ToResponse();

            response.Id.Should().Be(auditEvent.Id);
            response.Message.Should().Be(auditEvent.Message);
            response.CreatedAt.Should().Be(auditEvent.CreatedAt);
            response.EventType.Should().Be(auditEvent.EventType);
            response.SocialCareId.Should().Be(auditEvent.SocialCareId);
            response.UserId.Should().Be(auditEvent.UserId);
            response.Metadata.Should().BeEquivalentTo(JObject.Parse(auditEvent.Metadata));
        }

        [Test]
        public void PageMetadataMapsCorrectly()
        {
            var pageMetadata = _fixture.Create<IPagedList>();

            var response = pageMetadata.ToResponse();

            response.PageCount.Should().Be(pageMetadata.PageCount);
            response.TotalItemCount.Should().Be(pageMetadata.TotalItemCount);
            response.PageNumber.Should().Be(pageMetadata.PageNumber);
            response.PageSize.Should().Be(pageMetadata.PageSize);
            response.HasPreviousPage.Should().Be(pageMetadata.HasPreviousPage);
            response.HasNextPage.Should().Be(pageMetadata.HasNextPage);
            response.IsFirstPage.Should().Be(pageMetadata.IsFirstPage);
            response.IsLastPage.Should().Be(pageMetadata.IsLastPage);
            response.FirstItemOnPage.Should().Be(pageMetadata.FirstItemOnPage);
            response.LastItemOnPage.Should().Be(pageMetadata.LastItemOnPage);
        }
    }
}
