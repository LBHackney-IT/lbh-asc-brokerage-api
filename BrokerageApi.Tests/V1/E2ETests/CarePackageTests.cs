using System;
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
using FluentAssertions.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using NUnit.Framework;

namespace BrokerageApi.Tests.V1.E2ETests
{
    public class CarePackageTests : IntegrationTests<Startup>
    {
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            AssertionOptions.EquivalencySteps.Insert<InstantComparer>();
            _fixture = FixtureHelpers.Fixture;
        }

        [Test, Property("AsUser", "Broker")]
        public async Task CanGetCarePackage()
        {
            // Arrange
            var service = _fixture.BuildService()
                .Create();

            var hourlyElementType = _fixture.BuildElementType(service.Id)
                .With(et => et.CostType, ElementCostType.Hourly)
                .With(et => et.NonPersonalBudget, false)
                .Create();

            var dailyElementType = _fixture.BuildElementType(service.Id)
                .With(et => et.CostType, ElementCostType.Daily)
                .With(et => et.NonPersonalBudget, false)
                .Create();

            var provider = _fixture.BuildProvider()
                .Create();

            var providerService = _fixture.BuildProviderService(provider.Id, service.Id)
                .Create();

            var previousStartDate = CurrentDate.PlusDays(-100);
            var startDate = CurrentDate.PlusDays(1);

            var parentElement = _fixture.BuildElement(provider.Id, hourlyElementType.Id)
                .Create();

            var elementWithSuspensions = _fixture.BuildElement(provider.Id, hourlyElementType.Id)
                .With(e => e.StartDate, startDate)
                .WithoutCost()
                .Create();

            var suspensions = new[]
            {
                new Element(elementWithSuspensions)
                {
                    StartDate = elementWithSuspensions.StartDate.PlusDays(1),
                    EndDate = elementWithSuspensions.StartDate.PlusDays(2),
                    SuspendedElementId = elementWithSuspensions.Id, IsSuspension = true
                },
                new Element(elementWithSuspensions)
                {
                    StartDate = elementWithSuspensions.StartDate.PlusDays(5),
                    EndDate = elementWithSuspensions.StartDate.PlusDays(7),
                    SuspendedElementId = elementWithSuspensions.Id, IsSuspension = true
                }
            };

            var element1 = new Element
            {
                SocialCareId = "33556688",
                ElementTypeId = hourlyElementType.Id,
                NonPersonalBudget = false,
                ProviderId = provider.Id,
                Details = "Some notes",
                InternalStatus = ElementStatus.Approved,
                ParentElementId = null,
                StartDate = previousStartDate,
                EndDate = null,
                Monday = new ElementCost(3, 75),
                Tuesday = new ElementCost(3, 75),
                Thursday = new ElementCost(3, 75),
                Quantity = 6,
                Cost = 225,
                CreatedAt = CurrentInstant,
                UpdatedAt = CurrentInstant,
                ParentElement = parentElement
            };

            var element2 = new Element
            {
                SocialCareId = "33556688",
                ElementTypeId = dailyElementType.Id,
                NonPersonalBudget = false,
                ProviderId = provider.Id,
                Details = "Some other notes",
                InternalStatus = ElementStatus.InProgress,
                ParentElementId = null,
                StartDate = startDate,
                EndDate = null,
                Wednesday = new ElementCost(1, -100),
                Quantity = 1,
                Cost = -100,
                CreatedAt = CurrentInstant,
                UpdatedAt = CurrentInstant,
                ParentElement = parentElement
            };

            var referral = new Referral()
            {
                WorkflowId = "3e48adb1-0ca2-456c-845a-efcd4eca4554",
                WorkflowType = WorkflowType.Assessment,
                FormName = "Care act assessment",
                SocialCareId = "33556688",
                ResidentName = "A Service User",
                PrimarySupportReason = "Physical Support",
                AssignedTo = "a.broker@hackney.gov.uk",
                Status = ReferralStatus.InProgress,
                StartedAt = CurrentInstant,
                CreatedAt = PreviousInstant,
                UpdatedAt = CurrentInstant,
                Elements = new List<Element>
                {
                    element1,
                    element2,
                    elementWithSuspensions
                }
            };

            await Context.Services.AddAsync(service);
            await Context.ElementTypes.AddAsync(hourlyElementType);
            await Context.ElementTypes.AddAsync(dailyElementType);
            await Context.Elements.AddAsync(parentElement);
            await Context.Elements.AddRangeAsync(suspensions);
            await Context.Providers.AddAsync(provider);
            await Context.ProviderServices.AddAsync(providerService);
            await Context.Referrals.AddAsync(referral);
            await Context.SaveChangesAsync();

            Context.ChangeTracker.Clear();

            // Act
            var (code, response) = await Get<CarePackageResponse>($"/api/v1/referrals/{referral.Id}/care-package");

            // Assert
            code.Should().Be(HttpStatusCode.OK);

            response.Id.Should().Be(referral.Id);
            response.StartDate.Should().Be(previousStartDate);
            response.WeeklyCost.Should().Be(225);
            response.WeeklyPayment.Should().Be(125);
            response.Elements.Should().HaveCount(3);

            var responseElement1 = response.Elements.Single(e => e.Id == element1.Id);
            ValidateElementResponse(responseElement1, element1, hourlyElementType, service, provider, parentElement);
            responseElement1.Status.Should().Be(ElementStatus.Active);

            var responseElement2 = response.Elements.Single(e => e.Id == element2.Id);
            ValidateElementResponse(responseElement2, element2, dailyElementType, service, provider, parentElement);
            responseElement2.Status.Should().Be(ElementStatus.InProgress);

            var responseElement3 = response.Elements.Single(e => e.Id == elementWithSuspensions.Id);
            ValidateElementResponse(responseElement3, elementWithSuspensions, hourlyElementType, service, provider, null);
            responseElement3.SuspensionElements.Should().BeEquivalentTo(suspensions.Select(e => e.ToResponse()));
        }
        private static void ValidateElementResponse(ElementResponse elementResponse, Element element, ElementType elementType, Service service, Provider provider, Element parentElement)
        {
            elementResponse.Details.Should().Be(element.Details);
            elementResponse.ElementType.Name.Should().Be(elementType.Name);
            elementResponse.ElementType.Service.Name.Should().Be(service.Name);
            elementResponse.Provider.Name.Should().Be(provider.Name);
            if (parentElement != null)
            {
                elementResponse.ParentElement.Should().BeEquivalentTo(parentElement.ToResponse());
            }
        }

        [Test, Property("AsUser", "Broker")]
        public async Task CanStartCarePackage()
        {
            // Arrange
            var referral = new Referral()
            {
                WorkflowId = "3a386bf5-036d-47eb-ba58-704f3333e4fd",
                WorkflowType = WorkflowType.Assessment,
                FormName = "Care act assessment",
                SocialCareId = "33556688",
                ResidentName = "A Service User",
                PrimarySupportReason = "Physical Support",
                DirectPayments = "No",
                Status = ReferralStatus.Assigned,
                AssignedTo = "api.user@hackney.gov.uk",
                StartedAt = null,
                CreatedAt = PreviousInstant,
                UpdatedAt = PreviousInstant
            };

            await Context.Referrals.AddAsync(referral);
            await Context.SaveChangesAsync();

            Context.ChangeTracker.Clear();

            // Act
            var (code, response) = await Post<ReferralResponse>($"/api/v1/referrals/{referral.Id}/care-package/start", null);

            // Assert
            code.Should().Be(HttpStatusCode.OK);
            response.Status.Should().Be(ReferralStatus.InProgress);
            response.StartedAt.Should().Be(CurrentInstant);
            response.UpdatedAt.Should().BeEquivalentTo(CurrentInstant);
        }

        [Test, Property("AsUser", "Broker")]
        public async Task CanEndCarePackage()
        {
            // Arrange
            var endDate = CurrentDate.PlusDays(-1);
            var service = _fixture.BuildService()
                .Create();

            var provider = _fixture.BuildProvider()
                .Create();

            var providerService = _fixture.BuildProviderService(provider.Id, service.Id)
                .Create();

            var elementType = _fixture.BuildElementType(service.Id)
                .Create();

            var elements = _fixture.BuildElement(provider.Id, elementType.Id)
                .With(e => e.InternalStatus, ElementStatus.Approved)
                .With(e => e.StartDate, endDate.PlusDays(-5))
                .With(e => e.EndDate, endDate.PlusDays(5))
                .CreateMany();

            var referral = _fixture.BuildReferral(ReferralStatus.InProgress)
                .With(r => r.Status, ReferralStatus.Approved)
                .With(r => r.AssignedTo, ApiUser.Email)
                .With(r => r.Elements, elements.ToList)
                .Create();

            await Context.Referrals.AddAsync(referral);
            await Context.Services.AddAsync(service);
            await Context.Providers.AddAsync(provider);
            await Context.ProviderServices.AddAsync(providerService);
            await Context.ElementTypes.AddAsync(elementType);
            await Context.SaveChangesAsync();

            Context.ChangeTracker.Clear();

            var request = _fixture.Build<EndRequest>()
                .With(r => r.EndDate, endDate)
                .Create();

            var code = await Post($"/api/v1/referrals/{referral.Id}/care-package/end", request);

            code.Should().Be(HttpStatusCode.OK);

            var (carePackageCode, response) = await Get<CarePackageResponse>($"/api/v1/referrals/{referral.Id}/care-package");

            carePackageCode.Should().Be(HttpStatusCode.OK);
            response.Status.Should().Be(ReferralStatus.Ended);
            response.UpdatedAt.Should().BeEquivalentTo(CurrentInstant);

            response.Elements.Should().OnlyContain(e => e.EndDate <= endDate);
            response.Elements.Should().OnlyContain(e => e.Status == ElementStatus.Ended);
            response.Elements.Should().OnlyContain(e => e.UpdatedAt.IsSameOrEqualTo(CurrentInstant));
            Context.AuditEvents.Should().ContainSingle(ae => ae.EventType == AuditEventType.CarePackageEnded);
        }

        [Test, Property("AsUser", "Broker")]
        public async Task CanCancelCarePackage()
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

            var elements = _fixture.BuildElement(provider.Id, elementType.Id)
                .With(e => e.InternalStatus, ElementStatus.Approved)
                .CreateMany();

            var referral = _fixture.BuildReferral(ReferralStatus.InProgress)
                .With(r => r.Status, ReferralStatus.Approved)
                .With(r => r.AssignedTo, ApiUser.Email)
                .With(r => r.Elements, elements.ToList)
                .Create();

            await Context.Referrals.AddAsync(referral);
            await Context.Services.AddAsync(service);
            await Context.Providers.AddAsync(provider);
            await Context.ProviderServices.AddAsync(providerService);
            await Context.ElementTypes.AddAsync(elementType);
            await Context.SaveChangesAsync();

            Context.ChangeTracker.Clear();

            var request = _fixture.Build<CancelRequest>()
                .Create();

            var code = await Post($"/api/v1/referrals/{referral.Id}/care-package/cancel", request);

            code.Should().Be(HttpStatusCode.OK);

            var (carePackageCode, response) = await Get<CarePackageResponse>($"/api/v1/referrals/{referral.Id}/care-package");

            carePackageCode.Should().Be(HttpStatusCode.OK);
            response.Status.Should().Be(ReferralStatus.Cancelled);
            response.UpdatedAt.Should().BeEquivalentTo(CurrentInstant);
            response.Comment.Should().Be(request.Comment);

            response.Elements.Should().OnlyContain(e => e.Status == ElementStatus.Cancelled);
            response.Elements.Should().OnlyContain(e => e.UpdatedAt.IsSameOrEqualTo(CurrentInstant));
            Context.AuditEvents.Should().ContainSingle(ae => ae.EventType == AuditEventType.CarePackageCancelled);
        }

        [Test, Property("AsUser", "Broker")]
        public async Task CanSuspendCarePackage()
        {
            // Arrange
            var startDate = CurrentDate;
            var endDate = CurrentDate.PlusDays(2);
            var service = _fixture.BuildService()
                .Create();

            var provider = _fixture.BuildProvider()
                .Create();

            var providerService = _fixture.BuildProviderService(provider.Id, service.Id)
                .Create();

            var elementType = _fixture.BuildElementType(service.Id)
                .Create();

            var elements = _fixture.BuildElement(provider.Id, elementType.Id)
                .With(e => e.InternalStatus, ElementStatus.Approved)
                .With(e => e.StartDate, startDate.PlusDays(-5))
                .With(e => e.EndDate, endDate.PlusDays(5))
                .CreateMany();

            var referral = _fixture.BuildReferral(ReferralStatus.InProgress)
                .With(r => r.Status, ReferralStatus.Approved)
                .With(r => r.AssignedTo, ApiUser.Email)
                .With(r => r.Elements, elements.ToList)
                .Create();

            await Context.Referrals.AddAsync(referral);
            await Context.Services.AddAsync(service);
            await Context.Providers.AddAsync(provider);
            await Context.ProviderServices.AddAsync(providerService);
            await Context.ElementTypes.AddAsync(elementType);
            await Context.SaveChangesAsync();

            Context.ChangeTracker.Clear();

            var request = _fixture.Build<SuspendRequest>()
                .With(r => r.StartDate, startDate)
                .With(r => r.EndDate, endDate)
                .Create();

            var code = await Post($"/api/v1/referrals/{referral.Id}/care-package/suspend", request);

            code.Should().Be(HttpStatusCode.OK);

            var (carePackageCode, response) = await Get<CarePackageResponse>($"/api/v1/referrals/{referral.Id}/care-package");

            carePackageCode.Should().Be(HttpStatusCode.OK);
            response.UpdatedAt.Should().BeEquivalentTo(CurrentInstant);

            foreach (var element in elements)
            {
                var suspensionElement = await Context.Elements.SingleOrDefaultAsync(e => e.SuspendedElementId == element.Id);
                suspensionElement.Should().NotBeNull();
                suspensionElement.IsSuspension.Should().BeTrue();
            }
            Context.AuditEvents.Should().ContainSingle(ae => ae.EventType == AuditEventType.CarePackageSuspended);
        }
    }
}
