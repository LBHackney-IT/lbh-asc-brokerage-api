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
            var elements = _fixture.BuildElement(elementType.Id, provider.Id).CreateMany();

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
            var parentElement = _fixture.BuildElement(elementType.Id, provider.Id).Create();
            var childElement = _fixture.BuildElement(elementType.Id, provider.Id)
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
            var parentElement = _fixture.BuildElement(elementType.Id, provider.Id).Create();
            var childElement = _fixture.BuildElement(elementType.Id, provider.Id)
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
            var element = _fixture.BuildElement(elementType.Id, provider.Id)
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

            var request = new EndRequest
            {
                EndDate = CurrentDate
            };

            var code = await Post($"/api/v1/referrals/{referral.Id}/care-package/elements/{element.Id}/end", request);

            code.Should().Be(HttpStatusCode.OK);

            var resultReferralElement = await Context.ReferralElements.SingleAsync(re => re.ElementId == element.Id && re.ReferralId == referral.Id);
            resultReferralElement.PendingEndDate.Should().Be(request.EndDate);
            resultReferralElement.PendingComment.Should().Be(request.Comment);
        }

        [Test, Property("AsUser", "Broker")]
        public async Task CanCancelElement()
        {
            var provider = _fixture.BuildProvider().Create();
            var service = _fixture.BuildService().Create();
            var providerService = _fixture.BuildProviderService(provider.Id, service.Id).Create();
            var elementType = _fixture.BuildElementType(service.Id).Create();
            var referral = _fixture.BuildReferral(ReferralStatus.InProgress).Create();
            var element = _fixture.BuildElement(elementType.Id, provider.Id)
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

            var request = new CancelRequest
            {
                Comment = "here is a comment"
            };

            var code = await Post($"/api/v1/referrals/{referral.Id}/care-package/elements/{element.Id}/cancel", request);

            code.Should().Be(HttpStatusCode.OK);

            var resultReferralElement = await Context.ReferralElements.SingleAsync(re => re.ElementId == element.Id && re.ReferralId == referral.Id);
            resultReferralElement.PendingCancellation.Should().BeTrue();
            resultReferralElement.PendingComment.Should().Be(request.Comment);

            // reset
            var resetCode = await Post($"/api/v1/referrals/{referral.Id}/care-package/elements/{element.Id}/reset", null);

            resetCode.Should().Be(HttpStatusCode.OK);

            var resetReferralElement = await Context.ReferralElements.SingleAsync(re => re.ElementId == element.Id && re.ReferralId == referral.Id);
            resetReferralElement.PendingCancellation.Should().BeNull();
            resetReferralElement.PendingComment.Should().BeNull();
        }

        [Test, Property("AsUser", "Broker")]
        public async Task CanSuspendElement()
        {
            var provider = _fixture.BuildProvider().Create();
            var service = _fixture.BuildService().Create();
            var providerService = _fixture.BuildProviderService(provider.Id, service.Id).Create();
            var elementType = _fixture.BuildElementType(service.Id).Create();
            var referral = _fixture.BuildReferral(ReferralStatus.InProgress).Create();
            var element = _fixture.BuildElement(elementType.Id, provider.Id)
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

            await RequestSuspension(element, referral, true);
            await RequestSuspension(element, referral, false);

            var auditEvents = Context.AuditEvents.Where(ae => ae.EventType == AuditEventType.ElementSuspended);
            auditEvents.Should().HaveCount(2);

            var (carePackageCode, carePackageResponse) = await Get<CarePackageResponse>($"/api/v1/referrals/{referral.Id}/care-package");

            carePackageCode.Should().Be(HttpStatusCode.OK);
            carePackageResponse.Elements.Should().HaveCount(3);

            var resultElement = carePackageResponse.Elements.Single(e => e.Id == element.Id);
            resultElement.Id.Should().Be(element.Id);
            resultElement.SuspensionElements.Should().HaveCount(2);
            resultElement.SuspensionElements.Should().OnlyContain(e => e.IsSuspension);

            // reset
            var resetCode = await Post($"/api/v1/referrals/{referral.Id}/care-package/elements/{element.Id}/reset", null);

            resetCode.Should().Be(HttpStatusCode.OK);

            var resetElement = Context.Elements.Single(e => e.Id == element.Id);
            resetElement.Id.Should().Be(element.Id);
            resetElement.SuspensionElements.Should().BeEmpty();
        }

        [Test, Property("AsUser", "Broker")]
        public async Task CanDeleteSuspensionElement()
        {
            var provider = _fixture.BuildProvider().Create();
            var service = _fixture.BuildService().Create();
            var providerService = _fixture.BuildProviderService(provider.Id, service.Id).Create();
            var elementType = _fixture.BuildElementType(service.Id).Create();
            var referral = _fixture.BuildReferral(ReferralStatus.InProgress)
                .With(r => r.AssignedBrokerEmail, ApiUser.Email)
                .Create();
            var element = _fixture.BuildElement(elementType.Id, provider.Id)
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

            await RequestSuspension(element, referral, true);

            var (carePackageCode, carePackageResponse) = await Get<CarePackageResponse>($"/api/v1/referrals/{referral.Id}/care-package");

            carePackageCode.Should().Be(HttpStatusCode.OK);
            carePackageResponse.Elements.Should().HaveCount(2);

            var resultElement = carePackageResponse.Elements.Single(e => e.Id == element.Id);
            resultElement.SuspensionElements.Should().HaveCount(1);

            var suspensionElement = resultElement.SuspensionElements.Single();

            var deleteCode = await Delete($"/api/v1/referrals/{referral.Id}/care-package/elements/{suspensionElement.Id}");
            deleteCode.Should().Be(HttpStatusCode.OK);

            var (carePackageCode2, carePackageResponse2) = await Get<CarePackageResponse>($"/api/v1/referrals/{referral.Id}/care-package");

            carePackageCode2.Should().Be(HttpStatusCode.OK);
            carePackageResponse2.Elements.Should().HaveCount(1);

            var resultElement2 = carePackageResponse2.Elements.Single();
            resultElement2.Id.Should().Be(element.Id);
            resultElement2.SuspensionElements.Should().BeEmpty();
        }

        [Test, Property("AsUser", "Broker")]
        public async Task CanEndSuspensionElement()
        {
            var provider = _fixture.BuildProvider().Create();
            var service = _fixture.BuildService().Create();
            var providerService = _fixture.BuildProviderService(provider.Id, service.Id).Create();
            var elementType = _fixture.BuildElementType(service.Id).Create();
            var referral = _fixture.BuildReferral(ReferralStatus.InProgress)
                .With(r => r.AssignedBrokerEmail, ApiUser.Email)
                .Create();

            var element = _fixture.BuildElement(elementType.Id, provider.Id)
                .With(e => e.InternalStatus, ElementStatus.Approved)
                .With(e => e.StartDate, CurrentDate.PlusDays(-100))
                .Without(e => e.EndDate)
                .Create();
            var referralElement = _fixture.BuildReferralElement(referral.Id, element.Id).Create();

            var suspensionElement = _fixture.BuildElement(elementType.Id, provider.Id)
                .With(e => e.InternalStatus, ElementStatus.Approved)
                .With(e => e.StartDate, element.StartDate.PlusDays(10))
                .With(e => e.EndDate, CurrentDate.PlusDays(17))
                .With(e => e.SuspendedElementId, element.Id)
                .Create();
            var suspensionReferralElement = _fixture.BuildReferralElement(referral.Id, suspensionElement.Id).Create();

            await Context.Services.AddAsync(service);
            await Context.ElementTypes.AddAsync(elementType);
            await Context.Providers.AddAsync(provider);
            await Context.ProviderServices.AddAsync(providerService);
            await Context.Referrals.AddAsync(referral);
            await Context.Elements.AddRangeAsync(element);
            await Context.ReferralElements.AddAsync(referralElement);
            await Context.Elements.AddRangeAsync(suspensionElement);
            await Context.ReferralElements.AddAsync(suspensionReferralElement);
            await Context.SaveChangesAsync();

            Context.ChangeTracker.Clear();

            var request = new EndRequest
            {
                EndDate = CurrentDate
            };

            var code = await Post($"/api/v1/referrals/{referral.Id}/care-package/elements/{suspensionElement.Id}/end", request);

            code.Should().Be(HttpStatusCode.OK);

            var resultReferralElement = await Context.ReferralElements.SingleAsync(re => re.ElementId == suspensionElement.Id && re.ReferralId == referral.Id);
            resultReferralElement.PendingEndDate.Should().Be(request.EndDate);
            resultReferralElement.PendingComment.Should().Be(request.Comment);
        }

        [Test, Property("AsUser", "BrokerAndApprover")]
        public async Task CanCancelSuspensionElement()
        {
            var provider = _fixture.BuildProvider().Create();
            var service = _fixture.BuildService().Create();
            var providerService = _fixture.BuildProviderService(provider.Id, service.Id).Create();
            var elementType = _fixture.BuildElementType(service.Id).Create();
            var referral = _fixture.BuildReferral(ReferralStatus.InProgress)
                .With(r => r.AssignedBrokerEmail, ApiUser.Email)
                .Create();

            var element = _fixture.BuildElement(elementType.Id, provider.Id)
                .With(e => e.InternalStatus, ElementStatus.Approved)
                .Without(e => e.EndDate)
                .Create();
            var referralElement = _fixture.BuildReferralElement(referral.Id, element.Id).Create();

            var suspensionElement = _fixture.BuildElement(elementType.Id, provider.Id)
                .With(e => e.InternalStatus, ElementStatus.Approved)
                .With(e => e.StartDate, element.StartDate.PlusDays(10))
                .With(e => e.EndDate, element.StartDate.PlusDays(17))
                .With(e => e.SuspendedElementId, element.Id)
                .Create();
            var suspensionReferralElement = _fixture.BuildReferralElement(referral.Id, suspensionElement.Id).Create();

            await Context.Services.AddAsync(service);
            await Context.ElementTypes.AddAsync(elementType);
            await Context.Providers.AddAsync(provider);
            await Context.ProviderServices.AddAsync(providerService);
            await Context.Referrals.AddAsync(referral);
            await Context.Elements.AddRangeAsync(element);
            await Context.ReferralElements.AddAsync(referralElement);
            await Context.Elements.AddRangeAsync(suspensionElement);
            await Context.ReferralElements.AddAsync(suspensionReferralElement);
            await Context.SaveChangesAsync();

            Context.ChangeTracker.Clear();

            var request = new CancelRequest
            {
                Comment = "here is a comment"
            };

            var code = await Post($"/api/v1/referrals/{referral.Id}/care-package/elements/{suspensionElement.Id}/cancel", request);

            code.Should().Be(HttpStatusCode.OK);

            var resultReferralElement = await Context.ReferralElements.SingleAsync(re => re.ElementId == suspensionElement.Id && re.ReferralId == referral.Id);
            resultReferralElement.PendingCancellation.Should().BeTrue();
            resultReferralElement.PendingComment.Should().Be(request.Comment);

            var (carePackageCode, carePackageResponse) = await Get<CarePackageResponse>($"/api/v1/referrals/{referral.Id}/care-package");

            carePackageCode.Should().Be(HttpStatusCode.OK);
            var resultSuspensionElement = carePackageResponse.Elements.Single(e => e.Id == suspensionElement.Id);
            resultSuspensionElement.PendingCancellation.Should().BeTrue();
            resultSuspensionElement.PendingComment.Should().Be(request.Comment);

        }

        private async Task RequestSuspension(Element element, Referral referral, bool withEndDate)
        {
            var start = element.StartDate.PlusDays(_fixture.CreateInt(1, 100));
            var end = withEndDate ? start.PlusDays(_fixture.CreateInt(1, 100)) : (LocalDate?) null;
            var request = new SuspendRequest
            {
                StartDate = start,
                EndDate = end
            };

            var code = await Post($"/api/v1/referrals/{referral.Id}/care-package/elements/{element.Id}/suspend", request);

            code.Should().Be(HttpStatusCode.OK);

            Context.Elements.Should().ContainSingle(e =>
                e.SuspendedElementId == element.Id &&
                e.StartDate == request.StartDate &&
                e.EndDate == request.EndDate
            ).Which.UpdatedAt.Should().Be(CurrentInstant);
        }
    }
}
