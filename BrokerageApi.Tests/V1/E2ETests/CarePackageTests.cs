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
        public async Task CanStartCarePackage()
        {
            var referral = new Referral()
            {
                WorkflowId = "3a386bf5-036d-47eb-ba58-704f3333e4fd",
                WorkflowType = WorkflowType.Assessment,
                FormName = "Care act assessment",
                SocialCareId = "33556688",
                ResidentName = "A Service User",
                PrimarySupportReason = "Physical Support",
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
        }
    }
}