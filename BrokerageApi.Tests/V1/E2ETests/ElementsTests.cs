using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoFixture;
using BrokerageApi.Tests.V1.Helpers;
using BrokerageApi.V1.Boundary.Request;
using BrokerageApi.V1.Boundary.Response;
using BrokerageApi.V1.Factories;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.Infrastructure.AuditEvents;
using FluentAssertions;
using FluentAssertions.Equivalency;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using NUnit.Framework;

namespace BrokerageApi.Tests.V1.E2ETests
{
    public class InstantComparer : IEquivalencyStep
    {
        public bool CanHandle(IEquivalencyValidationContext context, IEquivalencyAssertionOptions config)
        {
            return context.Subject is Instant && context.Expectation is Instant;
        }

        public bool Handle(IEquivalencyValidationContext context, IEquivalencyValidator parent, IEquivalencyAssertionOptions config)
        {
            var a = (Instant) context.Subject;
            var b = (Instant) context.Expectation;

            return a.ToUnixTimeMilliseconds() == b.ToUnixTimeMilliseconds();
        }
    }

    public class ElementsTests : IntegrationTests<Startup>
    {
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _fixture = FixtureHelpers.Fixture;
        }

        [Test, Property("AsUser", "Broker")]
        public async Task CanGetElements()
        {
            var provider = _fixture.BuildProvider().Create();
            var service = _fixture.BuildService().Create();
            var providerService = _fixture.BuildProviderService(provider.Id, service.Id).Create();
            var elementType = _fixture.BuildElementType(service.Id).Create();
            var elements = _fixture.BuildElement(provider.Id, elementType.Id).CreateMany();

            await Context.Services.AddAsync(service);
            await Context.ElementTypes.AddAsync(elementType);
            await Context.Providers.AddAsync(provider);
            await Context.ProviderServices.AddAsync(providerService);
            await Context.Elements.AddRangeAsync(elements);
            await Context.SaveChangesAsync();

            Context.ChangeTracker.Clear();

            var (code, response) = await Get<List<ElementResponse>>($"/api/v1/elements/current");

            code.Should().Be(HttpStatusCode.OK);
            AssertionOptions.EquivalencySteps.Insert<InstantComparer>();
            response.ToArray().Should().BeEquivalentTo(elements.OrderBy(e => e.Id).Select(e => e.ToResponse()));
        }

        [Test, Property("AsUser", "Broker")]
        public async Task CanGetElementsWithParent()
        {
            var provider = _fixture.BuildProvider().Create();
            var service = _fixture.BuildService().Create();
            var providerService = _fixture.BuildProviderService(provider.Id, service.Id).Create();
            var elementType = _fixture.BuildElementType(service.Id).Create();
            var parentElement = _fixture.BuildElement(provider.Id, elementType.Id).Create();
            var childElement = _fixture.BuildElement(provider.Id, elementType.Id)
                .With(e => e.ParentElementId, parentElement.Id)
                .Create();

            await Context.Services.AddAsync(service);
            await Context.ElementTypes.AddAsync(elementType);
            await Context.Providers.AddAsync(provider);
            await Context.ProviderServices.AddAsync(providerService);
            await Context.Elements.AddRangeAsync(parentElement, childElement);
            await Context.SaveChangesAsync();

            Context.ChangeTracker.Clear();

            var (code, response) = await Get<List<ElementResponse>>($"/api/v1/elements/current");

            code.Should().Be(HttpStatusCode.OK);
            AssertionOptions.EquivalencySteps.Insert<InstantComparer>();
            var resultChild = response.Single(e => e.Id == childElement.Id);
            resultChild.ParentElement.Should().BeEquivalentTo(parentElement.ToResponse());
        }

        [Test, Property("AsUser", "Broker")]
        public async Task CanGetElementsById()
        {
            var provider = _fixture.BuildProvider().Create();
            var service = _fixture.BuildService().Create();
            var providerService = _fixture.BuildProviderService(provider.Id, service.Id).Create();
            var elementType = _fixture.BuildElementType(service.Id).Create();
            var parentElement = _fixture.BuildElement(provider.Id, elementType.Id).Create();
            var childElement = _fixture.BuildElement(provider.Id, elementType.Id)
                .With(e => e.ParentElementId, parentElement.Id)
                .Create();

            await Context.Services.AddAsync(service);
            await Context.ElementTypes.AddAsync(elementType);
            await Context.Providers.AddAsync(provider);
            await Context.ProviderServices.AddAsync(providerService);
            await Context.Elements.AddRangeAsync(parentElement, childElement);
            await Context.SaveChangesAsync();

            Context.ChangeTracker.Clear();

            var (code, response) = await Get<ElementResponse>($"/api/v1/elements/{childElement.Id}");

            code.Should().Be(HttpStatusCode.OK);
            AssertionOptions.EquivalencySteps.Insert<InstantComparer>();
            response.ParentElement.Should().BeEquivalentTo(parentElement.ToResponse());
        }

        [Test, Property("AsUser", "Broker")]
        public async Task CanEndElement()
        {
            var provider = _fixture.BuildProvider().Create();
            var service = _fixture.BuildService().Create();
            var providerService = _fixture.BuildProviderService(provider.Id, service.Id).Create();
            var elementType = _fixture.BuildElementType(service.Id).Create();
            var referral = _fixture.BuildReferral(ReferralStatus.InProgress).Create();
            var element = _fixture.BuildElement(provider.Id, elementType.Id)
                .With(e => e.InternalStatus, ElementStatus.Approved)
                .Without(e => e.EndDate)
                .Create();
            var referralElement = _fixture.BuildReferralElement(referral.Id, element.Id).Create();

            await Context.Services.AddAsync(service);
            await Context.ElementTypes.AddAsync(elementType);
            await Context.Providers.AddAsync(provider);
            await Context.ProviderServices.AddAsync(providerService);
            await Context.Referrals.AddAsync(referral);
            await Context.Elements.AddRangeAsync(element);
            await Context.ReferralElements.AddAsync(referralElement);
            await Context.SaveChangesAsync();

            Context.ChangeTracker.Clear();

            var request = new EndElementRequest
            {
                EndDate = CurrentDate
            };

            var code = await Post($"/api/v1/referrals/{referral.Id}/care-package/elements/{element.Id}/end", request);

            code.Should().Be(HttpStatusCode.OK);

            var resultElement = await Context.Elements.SingleAsync(e => e.Id == element.Id);
            resultElement.EndDate.Should().Be(request.EndDate);
            resultElement.UpdatedAt.Should().Be(CurrentInstant);

            var auditEvent = await Context.AuditEvents.SingleOrDefaultAsync(ae => ae.EventType == AuditEventType.ElementEnded);
            auditEvent.Should().NotBeNull();
        }
    }
}
