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
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace BrokerageApi.Tests.V1.E2ETests
{
    public class CarePackageTests : IntegrationTests<Startup>
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test, Property("AsUser", "Broker")]
        public async Task CanGetCarePackage()
        {
            // Arrange
            var service = new Service
            {
                Id = 1,
                Name = "Supported Living",
                Position = 1,
                IsArchived = false,
            };

            var hourlyElementType = new ElementType
            {
                Id = 1,
                ServiceId = 1,
                Name = "Day Opportunities (hourly)",
                CostType = ElementCostType.Hourly,
                NonPersonalBudget = false,
                IsArchived = false
            };

            var dailyElementType = new ElementType
            {
                Id = 2,
                ServiceId = 1,
                Name = "Day Opportunities (daily)",
                CostType = ElementCostType.Daily,
                NonPersonalBudget = false,
                IsArchived = false
            };

            var provider = new Provider()
            {
                Id = 1,
                Name = "Acme Homes",
                Address = "1 Knowhere Road",
                Type = ProviderType.Framework
            };

            var providerService = new ProviderService()
            {
                ProviderId = 1,
                ServiceId = 1,
                SubjectiveCode = "599999"
            };

            var previousStartDate = CurrentDate.PlusDays(-100);
            var startDate = CurrentDate.PlusDays(1);

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
                    new Element
                    {
                        SocialCareId = "33556688",
                        ElementTypeId = 1,
                        NonPersonalBudget = false,
                        ProviderId = 1,
                        Details = "Some notes",
                        InternalStatus = ElementStatus.Approved,
                        RelatedElementId = null,
                        StartDate = previousStartDate,
                        EndDate = null,
                        Monday = new ElementCost(3, 75),
                        Tuesday = new ElementCost(3, 75),
                        Thursday = new ElementCost(3, 75),
                        Quantity = 6,
                        Cost = 225,
                        CreatedAt = CurrentInstant,
                        UpdatedAt = CurrentInstant
                    },
                    new Element
                    {
                        SocialCareId = "33556688",
                        ElementTypeId = 2,
                        NonPersonalBudget = false,
                        ProviderId = 1,
                        Details = "Some other notes",
                        InternalStatus = ElementStatus.InProgress,
                        RelatedElementId = null,
                        StartDate = startDate,
                        EndDate = null,
                        Wednesday = new ElementCost(1, -100),
                        Quantity = 1,
                        Cost = -100,
                        CreatedAt = CurrentInstant,
                        UpdatedAt = CurrentInstant
                    },
                }
            };

            await Context.Services.AddAsync(service);
            await Context.ElementTypes.AddAsync(hourlyElementType);
            await Context.ElementTypes.AddAsync(dailyElementType);
            await Context.Providers.AddAsync(provider);
            await Context.ProviderServices.AddAsync(providerService);
            await Context.Referrals.AddAsync(referral);
            await Context.SaveChangesAsync();

            Context.ChangeTracker.Clear();

            // Act
            var (code, response) = await Get<CarePackageResponse>($"/api/v1/referrals/{referral.Id}/care-package");

            // Assert
            Assert.That(code, Is.EqualTo(HttpStatusCode.OK));

            Assert.That(response.Id, Is.EqualTo(referral.Id));
            Assert.That(response.StartDate, Is.EqualTo(previousStartDate));
            Assert.That(response.WeeklyCost, Is.EqualTo(225));
            Assert.That(response.WeeklyPayment, Is.EqualTo(125));
            Assert.That(response.Elements.Count, Is.EqualTo(2));

            Assert.That(response.Elements[0].Status, Is.EqualTo(ElementStatus.Active));
            Assert.That(response.Elements[0].Details, Is.EqualTo("Some notes"));
            Assert.That(response.Elements[0].ElementType.Name, Is.EqualTo("Day Opportunities (hourly)"));
            Assert.That(response.Elements[0].ElementType.Service.Name, Is.EqualTo("Supported Living"));
            Assert.That(response.Elements[0].Provider.Name, Is.EqualTo("Acme Homes"));

            Assert.That(response.Elements[1].Status, Is.EqualTo(ElementStatus.InProgress));
            Assert.That(response.Elements[1].Details, Is.EqualTo("Some other notes"));
            Assert.That(response.Elements[1].ElementType.Name, Is.EqualTo("Day Opportunities (daily)"));
            Assert.That(response.Elements[1].ElementType.Service.Name, Is.EqualTo("Supported Living"));
            Assert.That(response.Elements[1].Provider.Name, Is.EqualTo("Acme Homes"));
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
            Assert.That(code, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Status, Is.EqualTo(ReferralStatus.InProgress));
            Assert.That(response.StartedAt, Is.EqualTo(CurrentInstant));
            Assert.That(response.UpdatedAt, Is.EqualTo(CurrentInstant));
        }

        [Test, Property("AsUser", "Broker")]
        public async Task CanCreateElement()
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
                Status = ReferralStatus.InProgress,
                AssignedTo = "api.user@hackney.gov.uk",
                StartedAt = PreviousInstant,
                CreatedAt = PreviousInstant,
                UpdatedAt = PreviousInstant
            };

            var service = new Service()
            {
                Id = 1,
                Name = "Residential Care",
                IsArchived = false,
                Position = 1
            };

            var provider = new Provider()
            {
                Id = 1,
                Name = "Acme Homes",
                Address = "1 Knowhere Road",
                Type = ProviderType.Framework
            };

            var providerService = new ProviderService()
            {
                ProviderId = 1,
                ServiceId = 1,
                SubjectiveCode = "599999"
            };

            var elementType = new ElementType
            {
                Id = 1,
                ServiceId = 1,
                Name = "Day Opportunities (hourly)",
                CostType = ElementCostType.Hourly,
                NonPersonalBudget = false,
                IsArchived = false
            };

            await Context.Referrals.AddAsync(referral);
            await Context.Services.AddAsync(service);
            await Context.Providers.AddAsync(provider);
            await Context.ProviderServices.AddAsync(providerService);
            await Context.ElementTypes.AddAsync(elementType);
            await Context.SaveChangesAsync();

            Context.ChangeTracker.Clear();

            var request = new CreateElementRequest()
            {
                ElementTypeId = 1,
                NonPersonalBudget = true,
                ProviderId = 1,
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
                Cost = 300
            };

            // Act
            var (code, response) = await Post<ElementResponse>($"/api/v1/referrals/{referral.Id}/care-package/elements", request);
            var (referralCode, referralResponse) = await Get<ReferralResponse>($"/api/v1/referrals/{referral.Id}");

            // Assert
            Assert.That(code, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.ElementType.Id, Is.EqualTo(elementType.Id));
            Assert.That(response.Provider.Id, Is.EqualTo(provider.Id));
            Assert.That(response.Details, Is.EqualTo("Some notes"));
            Assert.That(response.StartDate, Is.EqualTo(CurrentDate));
            Assert.That(response.Tuesday, Is.EqualTo(new ElementCost(3, 150)));
            Assert.That(response.Thursday, Is.EqualTo(new ElementCost(3, 150)));
            Assert.That(response.Quantity, Is.EqualTo(6));
            Assert.That(response.Cost, Is.EqualTo(300));
            Assert.That(response.Status, Is.EqualTo(ElementStatus.InProgress));
            Assert.That(response.UpdatedAt, Is.EqualTo(CurrentInstant));

            Assert.That(referralCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(referralResponse.UpdatedAt, Is.EqualTo(CurrentInstant));
        }

        [Test, Property("AsUser", "Broker")]
        public async Task CanDeleteElement()
        {
            var referral = new Referral()
            {
                WorkflowId = "3a386bf5-036d-47eb-ba58-704f3333e4fd",
                WorkflowType = WorkflowType.Assessment,
                FormName = "Care act assessment",
                SocialCareId = "33556688",
                ResidentName = "A Service User",
                PrimarySupportReason = "Physical Support",
                Status = ReferralStatus.InProgress,
                AssignedTo = "api.user@hackney.gov.uk",
                StartedAt = PreviousInstant,
                CreatedAt = PreviousInstant,
                UpdatedAt = PreviousInstant
            };

            var service = new Service()
            {
                Id = 1,
                Name = "Residential Care",
                IsArchived = false,
                Position = 1
            };

            var provider = new Provider()
            {
                Id = 1,
                Name = "Acme Homes",
                Address = "1 Knowhere Road",
                Type = ProviderType.Framework
            };

            var providerService = new ProviderService()
            {
                ProviderId = 1,
                ServiceId = 1,
                SubjectiveCode = "599999"
            };

            var elementType = new ElementType
            {
                Id = 1,
                ServiceId = 1,
                Name = "Day Opportunities (hourly)",
                CostType = ElementCostType.Hourly,
                NonPersonalBudget = false,
                IsArchived = false
            };

            var element = new Element
            {
                ElementTypeId = 1,
                NonPersonalBudget = true,
                ProviderId = 1,
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
                SocialCareId = "socialCareId"
            };
            referral.Elements = new List<Element>
            {
                element
            };

            await Context.Referrals.AddAsync(referral);
            await Context.Services.AddAsync(service);
            await Context.Providers.AddAsync(provider);
            await Context.ProviderServices.AddAsync(providerService);
            await Context.ElementTypes.AddAsync(elementType);
            await Context.SaveChangesAsync();

            Context.ChangeTracker.Clear();
            var elementId = element.Id;

            // Act
            var code = await Delete($"/api/v1/referrals/{referral.Id}/care-package/elements/{elementId}");

            // Assert
            code.Should().Be(HttpStatusCode.OK);
            Context.ReferralElements.Should().NotContain(re => re.ReferralId == referral.Id && re.ElementId == elementId);
        }
    }
}
