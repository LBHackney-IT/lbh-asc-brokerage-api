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
using Microsoft.EntityFrameworkCore;
using NodaTime;
using NUnit.Framework;

namespace BrokerageApi.Tests.V1.E2ETests
{
    public class CarePackageElementsTests : IntegrationTests<Startup>
    {
        private Fixture _fixture;
        [SetUp]
        public void Setup()
        {
            AssertionOptions.EquivalencySteps.Insert<InstantComparer>();
            _fixture = FixtureHelpers.Fixture;
        }

        [Test, Property("AsUser", "Broker")]
        public async Task CanCreateElement()
        {
            // Arrange
            var service = _fixture.BuildService()
                .Create();

            var provider = _fixture.BuildProvider()
                .Create();

            var providerService = _fixture.BuildProviderService(provider.Id, service.Id)
                .Create();

            var elementType = _fixture.BuildElementType(service.Id)
                .Create();

            var parentElement = _fixture.BuildElement(elementType.Id, provider.Id)
                .Create();

            var element = _fixture.BuildElement(elementType.Id, provider.Id)
                .With(e => e.ParentElement, parentElement)
                .Create();

            var referral = _fixture.BuildReferral(ReferralStatus.InProgress)
                .With(r => r.AssignedBrokerEmail, ApiUser.Email)
                .With(r => r.Elements, new List<Element> { element })
                .Create();

            await Context.Referrals.AddAsync(referral);
            await Context.Services.AddAsync(service);
            await Context.Providers.AddAsync(provider);
            await Context.ProviderServices.AddAsync(providerService);
            await Context.ElementTypes.AddAsync(elementType);
            await Context.SaveChangesAsync();

            Context.ChangeTracker.Clear();

            var request = _fixture.Build<CreateElementRequest>()
                .With(r => r.ElementTypeId, elementType.Id)
                .With(r => r.ProviderId, provider.Id)
                .With(r => r.StartDate, CurrentDate)
                .Without(r => r.ParentElementId)
                .Create();

            // Act
            var (code, response) = await Post<ElementResponse>($"/api/v1/referrals/{referral.Id}/care-package/elements", request);
            var (referralCode, referralResponse) = await Get<ReferralResponse>($"/api/v1/referrals/{referral.Id}");

            // Assert
            code.Should().Be(HttpStatusCode.OK);
            response.ElementType.Id.Should().Be(request.ElementTypeId);
            response.Provider.Id.Should().Be(request.ProviderId);
            response.Details.Should().Be(request.Details);
            response.StartDate.Should().Be(request.StartDate);
            response.Monday.Should().BeEquivalentTo(request.Monday);
            response.Tuesday.Should().BeEquivalentTo(request.Tuesday);
            response.Wednesday.Should().BeEquivalentTo(request.Wednesday);
            response.Thursday.Should().BeEquivalentTo(request.Thursday);
            response.Friday.Should().BeEquivalentTo(request.Friday);
            response.Quantity.Should().Be(request.Quantity);
            response.Cost.Should().Be(request.Cost);
            response.Status.Should().Be(ElementStatus.InProgress);
            response.UpdatedAt.Should().Be(CurrentInstant);
            response.CreatedBy.Should().Be("api.user@hackney.gov.uk");

            referralCode.Should().Be(HttpStatusCode.OK);
            referralResponse.UpdatedAt.Should().Be(CurrentInstant);
        }

        [Test, Property("AsUser", "Broker")]
        public async Task CanCreateElementWithoutProviderAndDetails()
        {
            // Arrange
            var service = _fixture.BuildService()
                .Create();

            var elementType = _fixture.BuildElementType(service.Id)
                .Create();

            var referral = _fixture.BuildReferral(ReferralStatus.InProgress)
                .With(r => r.AssignedBrokerEmail, ApiUser.Email)
                .Create();

            await Context.Referrals.AddAsync(referral);
            await Context.Services.AddAsync(service);
            await Context.ElementTypes.AddAsync(elementType);
            await Context.SaveChangesAsync();

            Context.ChangeTracker.Clear();

            var request = _fixture.Build<CreateElementRequest>()
                .With(r => r.ElementTypeId, elementType.Id)
                .With(r => r.StartDate, CurrentDate)
                .Without(r => r.ProviderId)
                .Without(r => r.Details)
                .Without(r => r.ParentElementId)
                .Create();

            // Act
            var (code, response) = await Post<ElementResponse>($"/api/v1/referrals/{referral.Id}/care-package/elements", request);
            var (referralCode, referralResponse) = await Get<ReferralResponse>($"/api/v1/referrals/{referral.Id}");

            // Assert
            code.Should().Be(HttpStatusCode.OK);
            response.ElementType.Id.Should().Be(request.ElementTypeId);
            response.Provider.Should().BeNull();
            response.Details.Should().BeNull();
            response.StartDate.Should().Be(request.StartDate);
            response.Monday.Should().BeEquivalentTo(request.Monday);
            response.Tuesday.Should().BeEquivalentTo(request.Tuesday);
            response.Wednesday.Should().BeEquivalentTo(request.Wednesday);
            response.Thursday.Should().BeEquivalentTo(request.Thursday);
            response.Friday.Should().BeEquivalentTo(request.Friday);
            response.Quantity.Should().Be(request.Quantity);
            response.Cost.Should().Be(request.Cost);
            response.Status.Should().Be(ElementStatus.InProgress);
            response.UpdatedAt.Should().Be(CurrentInstant);
            response.CreatedBy.Should().Be("api.user@hackney.gov.uk");

            referralCode.Should().Be(HttpStatusCode.OK);
            referralResponse.UpdatedAt.Should().Be(CurrentInstant);
        }

        [Test, Property("AsUser", "Broker")]
        public async Task CanDeleteElement()
        {
            var service = _fixture.BuildService()
                .Create();

            var provider = _fixture.BuildProvider()
                .Create();

            var providerService = _fixture.BuildProviderService(provider.Id, service.Id)
                .Create();

            var elementType = _fixture.BuildElementType(service.Id)
                .Create();

            var parentElement = _fixture.BuildElement(elementType.Id, provider.Id)
                .Create();

            var element = _fixture.BuildElement(elementType.Id, provider.Id)
                .With(e => e.ParentElement, parentElement)
                .Create();

            var referral = _fixture.BuildReferral(ReferralStatus.InProgress)
                .With(r => r.AssignedBrokerEmail, ApiUser.Email)
                .With(r => r.Elements, new List<Element> { element })
                .Create();

            await Context.Referrals.AddAsync(referral);
            await Context.Services.AddAsync(service);
            await Context.Providers.AddAsync(provider);
            await Context.ProviderServices.AddAsync(providerService);
            await Context.ElementTypes.AddAsync(elementType);
            await Context.SaveChangesAsync();

            Context.ChangeTracker.Clear();

            // Act
            var code = await Delete($"/api/v1/referrals/{referral.Id}/care-package/elements/{element.Id}");

            // Assert
            code.Should().Be(HttpStatusCode.OK);
            Context.ReferralElements.Should().NotContain(re => re.ReferralId == referral.Id && re.ElementId == element.Id);
            Context.ReferralElements.Should().Contain(re => re.ReferralId == referral.Id && re.ElementId == parentElement.Id);

            var resultReferral = await Context.Referrals.SingleAsync(r => r.Id == referral.Id);
            resultReferral.Elements.Should().NotContain(e => e.Id == element.Id);
            resultReferral.Elements.Should().Contain(e => e.Id == parentElement.Id);
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
        public async Task CanReplaceElement()
        {
            // Arrange
            var service = _fixture.BuildService()
                .Create();

            var provider = _fixture.BuildProvider()
                .Create();

            var providerService = _fixture.BuildProviderService(provider.Id, service.Id)
                .Create();

            var elementType = _fixture.BuildElementType(service.Id)
                .Create();

            var parentElement = _fixture.BuildElement(elementType.Id, provider.Id)
                .Create();

            var referral = _fixture.BuildReferral(ReferralStatus.InProgress)
                .With(r => r.Elements, new List<Element> { parentElement })
                .With(r => r.AssignedBrokerEmail, ApiUser.Email)
                .Create();

            await Context.Referrals.AddAsync(referral);
            await Context.Services.AddAsync(service);
            await Context.Providers.AddAsync(provider);
            await Context.ProviderServices.AddAsync(providerService);
            await Context.ElementTypes.AddAsync(elementType);
            await Context.Elements.AddAsync(parentElement);
            await Context.SaveChangesAsync();

            Context.ChangeTracker.Clear();

            var request = new CreateElementRequest()
            {
                ElementTypeId = elementType.Id,
                NonPersonalBudget = true,
                ProviderId = provider.Id,
                Details = "Some notes",
                StartDate = CurrentDate,
                EndDate = null,
                Monday = null,
                Tuesday = new ElementCost(3, 150),
                Wednesday = null,
                Thursday = new ElementCost(3, 150),
                Friday = null,
                Saturday = null,
                Sunday = null,
                Quantity = 6,
                Cost = 300,
                ParentElementId = parentElement.Id
            };

            // Act
            var (code, response) = await Post<ElementResponse>($"/api/v1/referrals/{referral.Id}/care-package/elements", request);

            // Assert
            code.Should().Be(HttpStatusCode.OK);

            response.Should().BeEquivalentTo(request.ToDatabase().ToResponse(), options => options
                .Excluding(e => e.Id)
                .Excluding(e => e.ElementType)
                .Excluding(e => e.Provider)
                .Excluding(e => e.ParentElement)
                .Excluding(e => e.CreatedAt)
                .Excluding(e => e.UpdatedAt)
                .Excluding(e => e.CreatedBy)
            );

            response.ParentElement.Should().BeEquivalentTo(parentElement.ToResponse(), options => options
                .Excluding(e => e.Id)
                .Excluding(e => e.ElementType)
                .Excluding(e => e.Provider)
                .Excluding(e => e.ParentElement)
                .Excluding(e => e.CreatedAt)
                .Excluding(e => e.UpdatedAt)
                .Excluding(e => e.CreatedBy)
            );

            var resultParentElement = await Context.Elements.SingleAsync(e => e.Id == parentElement.Id);
            resultParentElement.Referrals.Should().NotContain(r => r.Id == referral.Id);

            var resultReferral = await Context.Referrals.SingleAsync(r => r.Id == referral.Id);
            resultReferral.Elements.Should().NotContain(e => e.Id == parentElement.Id);

            var resultReferralElement = await Context.ReferralElements.SingleOrDefaultAsync(re => re.ElementId == parentElement.Id && re.ReferralId == referral.Id);
            resultReferralElement.Should().BeNull();

            // reset
            var resetCode = await Post($"/api/v1/referrals/{referral.Id}/care-package/elements/{response.Id}/reset", null);

            resetCode.Should().Be(HttpStatusCode.OK);

            var resetReferral = await Context.Referrals.SingleAsync(r => r.Id == referral.Id);
            resetReferral.Elements.Should().OnlyContain(e => e.Id == parentElement.Id);
        }

        [Test, Property("AsUser", "Broker")]
        public async Task CanEditElement()
        {
            // Arrange
            var service = _fixture.BuildService()
                .Create();

            var provider = _fixture.BuildProvider()
                .Create();

            var providerService = _fixture.BuildProviderService(provider.Id, service.Id)
                .Create();

            var elementType = _fixture.BuildElementType(service.Id)
                .Create();

            var parentElement = _fixture.BuildElement(elementType.Id, provider.Id)
                .Create();

            var element = _fixture.BuildElement(elementType.Id, provider.Id)
                .With(e => e.ParentElement, parentElement)
                .Create();

            var referral = _fixture.BuildReferral(ReferralStatus.InProgress)
                .With(r => r.AssignedBrokerEmail, ApiUser.Email)
                .With(r => r.Elements, new List<Element> { element })
                .Create();

            await Context.Referrals.AddAsync(referral);
            await Context.Services.AddAsync(service);
            await Context.Providers.AddAsync(provider);
            await Context.ProviderServices.AddAsync(providerService);
            await Context.ElementTypes.AddAsync(elementType);
            await Context.SaveChangesAsync();

            Context.ChangeTracker.Clear();

            var request = _fixture.Build<EditElementRequest>()
                .With(r => r.ElementTypeId, elementType.Id)
                .With(r => r.ProviderId, provider.Id)
                .With(r => r.StartDate, CurrentDate)
                .Create();

            // Act
            var (code, response) = await Post<ElementResponse>($"/api/v1/referrals/{referral.Id}/care-package/elements/{element.Id}/edit", request);
            var (referralCode, referralResponse) = await Get<ReferralResponse>($"/api/v1/referrals/{referral.Id}");

            // Assert
            code.Should().Be(HttpStatusCode.OK);
            response.ElementType.Id.Should().Be(request.ElementTypeId);
            response.Provider.Id.Should().Be(request.ProviderId);
            response.Details.Should().Be(request.Details);
            response.StartDate.Should().Be(request.StartDate);
            response.Monday.Should().BeEquivalentTo(request.Monday);
            response.Tuesday.Should().BeEquivalentTo(request.Tuesday);
            response.Wednesday.Should().BeEquivalentTo(request.Wednesday);
            response.Thursday.Should().BeEquivalentTo(request.Thursday);
            response.Friday.Should().BeEquivalentTo(request.Friday);
            response.Quantity.Should().Be(request.Quantity);
            response.Cost.Should().Be(request.Cost);
            response.Status.Should().Be(ElementStatus.InProgress);
            response.UpdatedAt.Should().Be(CurrentInstant);

            referralCode.Should().Be(HttpStatusCode.OK);
            referralResponse.UpdatedAt.Should().Be(CurrentInstant);
        }

        [Test, Property("AsUser", "Broker")]
        public async Task CanEditElementWithoutProviderAndDetails()
        {
            // Arrange
            var service = _fixture.BuildService()
                .Create();

            var provider = _fixture.BuildProvider()
                .Create();

            var providerService = _fixture.BuildProviderService(provider.Id, service.Id)
                .Create();

            var elementType = _fixture.BuildElementType(service.Id)
                .Create();

            var element = _fixture.BuildElement(elementType.Id, provider.Id)
                .Create();

            var referral = _fixture.BuildReferral(ReferralStatus.InProgress)
                .With(r => r.AssignedBrokerEmail, ApiUser.Email)
                .With(r => r.Elements, new List<Element> { element })
                .Create();

            await Context.Referrals.AddAsync(referral);
            await Context.Services.AddAsync(service);
            await Context.Providers.AddAsync(provider);
            await Context.ProviderServices.AddAsync(providerService);
            await Context.ElementTypes.AddAsync(elementType);
            await Context.SaveChangesAsync();

            Context.ChangeTracker.Clear();

            var request = _fixture.Build<EditElementRequest>()
                .With(r => r.ElementTypeId, elementType.Id)
                .With(r => r.StartDate, CurrentDate)
                .Without(r => r.ProviderId)
                .Without(r => r.Details)
                .Create();

            // Act
            var (code, response) = await Post<ElementResponse>($"/api/v1/referrals/{referral.Id}/care-package/elements/{element.Id}/edit", request);
            var (referralCode, referralResponse) = await Get<ReferralResponse>($"/api/v1/referrals/{referral.Id}");

            // Assert
            code.Should().Be(HttpStatusCode.OK);
            response.ElementType.Id.Should().Be(request.ElementTypeId);
            response.Provider.Should().BeNull();
            response.Details.Should().BeNull();
            response.StartDate.Should().Be(request.StartDate);
            response.Monday.Should().BeEquivalentTo(request.Monday);
            response.Tuesday.Should().BeEquivalentTo(request.Tuesday);
            response.Wednesday.Should().BeEquivalentTo(request.Wednesday);
            response.Thursday.Should().BeEquivalentTo(request.Thursday);
            response.Friday.Should().BeEquivalentTo(request.Friday);
            response.Quantity.Should().Be(request.Quantity);
            response.Cost.Should().Be(request.Cost);
            response.Status.Should().Be(ElementStatus.InProgress);
            response.UpdatedAt.Should().Be(CurrentInstant);

            referralCode.Should().Be(HttpStatusCode.OK);
            referralResponse.UpdatedAt.Should().Be(CurrentInstant);
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
