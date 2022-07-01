using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AutoFixture;
using BrokerageApi.Tests.V1.Helpers;
using BrokerageApi.V1.Boundary.Request;
using BrokerageApi.V1.Boundary.Response;
using BrokerageApi.V1.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace BrokerageApi.Tests.V1.E2ETests
{
    public class CarePackageCareChargesTests : IntegrationTests<Startup>
    {
        private Fixture _fixture;
        [SetUp]
        public void Setup()
        {
            AssertionOptions.EquivalencySteps.Insert<InstantComparer>();
            _fixture = FixtureHelpers.Fixture;
        }

        [Test, Property("AsUser", "CareChargesOfficer")]
        public async Task CanCreateCareCharge()
        {
            // Arrange
            var service = _fixture.BuildService()
                .Create();

            var elementType = _fixture.BuildElementType(service.Id, ElementTypeType.ConfirmedCareCharge)
                .Create();

            var parentElement = _fixture.BuildElement(elementType.Id)
                .Create();

            var element = _fixture.BuildElement(elementType.Id)
                .With(e => e.ParentElement, parentElement)
                .Create();

            var referral = _fixture.BuildReferral(ReferralStatus.Approved)
                .With(r => r.AssignedBrokerEmail, ApiUser.Email)
                .With(r => r.Elements, new List<Element> { element })
                .Create();

            await Context.Referrals.AddAsync(referral);
            await Context.Services.AddAsync(service);
            await Context.ElementTypes.AddAsync(elementType);
            await Context.SaveChangesAsync();

            Context.ChangeTracker.Clear();

            var request = _fixture.Build<CreateCareChargeRequest>()
                .With(r => r.ElementTypeId, elementType.Id)
                .With(r => r.StartDate, CurrentDate)
                .Without(r => r.ParentElementId)
                .Create();

            // Act
            var (code, response) = await Post<ElementResponse>($"/api/v1/referrals/{referral.Id}/care-package/care-charges", request);
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

        [Test, Property("AsUser", "CareChargesOfficer")]
        public async Task CanDeleteCareCharge()
        {
            var service = _fixture.BuildService()
                .Create();

            var elementType = _fixture.BuildElementType(service.Id, ElementTypeType.ConfirmedCareCharge)
                .Create();

            var parentElement = _fixture.BuildElement(elementType.Id)
                .Create();

            var element = _fixture.BuildElement(elementType.Id)
                .With(e => e.ParentElement, parentElement)
                .Create();

            var referral = _fixture.BuildReferral(ReferralStatus.Approved)
                .With(r => r.Elements, new List<Element> { element })
                .Create();

            await Context.Referrals.AddAsync(referral);
            await Context.Services.AddAsync(service);
            await Context.ElementTypes.AddAsync(elementType);
            await Context.SaveChangesAsync();

            Context.ChangeTracker.Clear();

            // Act
            var code = await Delete($"/api/v1/referrals/{referral.Id}/care-package/care-charges/{element.Id}");

            // Assert
            code.Should().Be(HttpStatusCode.OK);
            Context.ReferralElements.Should().NotContain(re => re.ReferralId == referral.Id && re.ElementId == element.Id);
            Context.ReferralElements.Should().Contain(re => re.ReferralId == referral.Id && re.ElementId == parentElement.Id);

            var resultReferral = await Context.Referrals.SingleAsync(r => r.Id == referral.Id);
            resultReferral.Elements.Should().NotContain(e => e.Id == element.Id);
            resultReferral.Elements.Should().Contain(e => e.Id == parentElement.Id);
        }

        [Test, Property("AsUser", "CareChargesOfficer")]
        public async Task CanEndCareCharge()
        {
            var service = _fixture.BuildService().Create();
            var elementType = _fixture.BuildElementType(service.Id, ElementTypeType.ConfirmedCareCharge).Create();
            var referral = _fixture.BuildReferral(ReferralStatus.Approved).Create();
            var element = _fixture.BuildElement(elementType.Id)
                .With(e => e.InternalStatus, ElementStatus.Approved)
                .Without(e => e.EndDate)
                .Create();
            var referralElement = _fixture.BuildReferralElement(referral.Id, element.Id).Create();

            await Context.Services.AddAsync(service);
            await Context.ElementTypes.AddAsync(elementType);
            await Context.Referrals.AddAsync(referral);
            await Context.Elements.AddRangeAsync(element);
            await Context.ReferralElements.AddAsync(referralElement);
            await Context.SaveChangesAsync();

            Context.ChangeTracker.Clear();

            var request = new EndRequest
            {
                EndDate = CurrentDate
            };

            var code = await Post($"/api/v1/referrals/{referral.Id}/care-package/care-charges/{element.Id}/end", request);

            code.Should().Be(HttpStatusCode.OK);

            var resultReferralElement = await Context.ReferralElements.SingleAsync(re => re.ElementId == element.Id && re.ReferralId == referral.Id);
            resultReferralElement.PendingEndDate.Should().Be(request.EndDate);
            resultReferralElement.PendingComment.Should().Be(request.Comment);
        }
    }
}
