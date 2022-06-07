using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using BrokerageApi.Tests.V1.Helpers;
using BrokerageApi.V1.Factories;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.Infrastructure.AuditEvents;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using X.PagedList;

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

        [Test]
        public void ElementMapsCorrectly()
        {
            var grandParentElement = _fixture.Build<Element>()
                .With(e => e.InternalStatus, ElementStatus.InProgress)
                .Create();
            var parentElement = _fixture.Build<Element>()
                .With(e => e.InternalStatus, ElementStatus.InProgress)
                .With(e => e.ParentElement, grandParentElement)
                .With(e => e.ParentElementId, grandParentElement.Id)
                .Create();
            var suspensionElements = _fixture.Build<Element>()
                .With(e => e.InternalStatus, ElementStatus.Suspended)
                .With(e => e.IsSuspension, true)
                .CreateMany();
            var element = _fixture.Build<Element>()
                .With(e => e.InternalStatus, ElementStatus.InProgress)
                .With(e => e.ParentElement, parentElement)
                .With(e => e.ParentElementId, parentElement.Id)
                .With(e => e.SuspensionElements, suspensionElements.ToList)
                .Create();

            var response = element.ToResponse();

            response.Id.Should().Be(element.Id);
            response.ElementType.Should().BeEquivalentTo(element.ElementType?.ToResponse());
            response.NonPersonalBudget.Should().Be(element.NonPersonalBudget);
            response.Provider.Should().BeEquivalentTo(element.Provider?.ToResponse());
            response.Details.Should().Be(element.Details);
            response.Status.Should().Be(element.Status);
            response.StartDate.Should().Be(element.StartDate);
            response.EndDate.Should().Be(element.EndDate);
            response.Monday.Should().Be(element.Monday);
            response.Tuesday.Should().Be(element.Tuesday);
            response.Wednesday.Should().Be(element.Wednesday);
            response.Thursday.Should().Be(element.Thursday);
            response.Friday.Should().Be(element.Friday);
            response.Saturday.Should().Be(element.Saturday);
            response.Sunday.Should().Be(element.Sunday);
            response.Quantity.Should().Be(element.Quantity);
            response.Cost.Should().Be(element.Cost);
            response.CreatedAt.Should().Be(element.CreatedAt);
            response.UpdatedAt.Should().Be(element.UpdatedAt);
            response.ParentElement.Should().BeEquivalentTo(parentElement.ToResponse(false));
            response.ParentElement.ParentElement.Should().BeNull();
            response.SuspensionElements.Should().BeEquivalentTo(suspensionElements.Select(e => e.ToResponse()));
            response.Comment.Should().Be(element.Comment);
        }
    }
}
