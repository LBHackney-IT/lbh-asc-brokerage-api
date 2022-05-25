using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AutoFixture;
using BrokerageApi.Tests.V1.Helpers;
using BrokerageApi.V1.Boundary.Request;
using BrokerageApi.V1.Boundary.Response;
using BrokerageApi.V1.Factories;
using BrokerageApi.V1.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
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

            var parentElement = _fixture.BuildElement(provider.Id, elementType.Id)
                .Create();

            var element = _fixture.BuildElement(provider.Id, elementType.Id)
                .With(e => e.ParentElement, parentElement)
                .Create();

            var referral = _fixture.BuildReferral(ReferralStatus.InProgress)
                .With(r => r.AssignedTo, ApiUser.Email)
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

            var parentElement = _fixture.BuildElement(provider.Id, elementType.Id)
                .Create();

            var element = _fixture.BuildElement(provider.Id, elementType.Id)
                .With(e => e.ParentElement, parentElement)
                .Create();

            var referral = _fixture.BuildReferral(ReferralStatus.InProgress)
                .With(r => r.AssignedTo, ApiUser.Email)
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

            var parentElement = _fixture.BuildElement(provider.Id, elementType.Id)
                .Create();

            var referral = _fixture.BuildReferral(ReferralStatus.InProgress)
                .With(r => r.Elements, new List<Element> { parentElement })
                .With(r => r.AssignedTo, ApiUser.Email)
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
            );

            response.ParentElement.Should().BeEquivalentTo(parentElement.ToResponse(), options => options
                .Excluding(e => e.Id)
                .Excluding(e => e.ElementType)
                .Excluding(e => e.Provider)
                .Excluding(e => e.ParentElement)
                .Excluding(e => e.CreatedAt)
                .Excluding(e => e.UpdatedAt)
            );

            var resultParentElement = await Context.Elements.SingleAsync(e => e.Id == parentElement.Id);
            resultParentElement.Referrals.Should().NotContain(r => r.Id == referral.Id);

            var resultReferral = await Context.Referrals.SingleAsync(r => r.Id == referral.Id);
            resultReferral.Elements.Should().NotContain(e => e.Id == parentElement.Id);

            var resultReferralElement = await Context.ReferralElements.SingleOrDefaultAsync(re => re.ElementId == parentElement.Id && re.ReferralId == referral.Id);
            resultReferralElement.Should().BeNull();
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

            var parentElement = _fixture.BuildElement(provider.Id, elementType.Id)
                .Create();

            var element = _fixture.BuildElement(provider.Id, elementType.Id)
                .With(e => e.ParentElement, parentElement)
                .Create();

            var referral = _fixture.BuildReferral(ReferralStatus.InProgress)
                .With(r => r.AssignedTo, ApiUser.Email)
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
    }
}
