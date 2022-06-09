using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using BrokerageApi.V1.Boundary.Response;
using BrokerageApi.V1.Infrastructure;
using NUnit.Framework;

namespace BrokerageApi.Tests.V1.E2ETests
{
    public class ServiceUserTests : IntegrationTests<Startup>
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test, Property("AsUser", "Broker")]
        public async Task CanGetCarePackagesByServiceUserId()
        {
            // Arrange
            var service = new Service
            {
                Id = 1,
                Name = "Supported Living",
                Position = 1,
                IsArchived = false,
            };

            var anotherService = new Service
            {
                Id = 2,
                Name = "A Different Service",
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
                ServiceId = 2,
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

            var anotherProvider = new Provider()
            {
                Id = 2,
                Name = "Testington Homes",
                Address = "1 Mystery Place",
                Type = ProviderType.Framework
            };

            var providerService = new ProviderService()
            {
                ProviderId = 1,
                ServiceId = 1,
                SubjectiveCode = "599999"
            };

            var anotherProviderService = new ProviderService()
            {
                ProviderId = 2,
                ServiceId = 2,
                SubjectiveCode = "599998"
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
                AssignedBroker = "some.email@hackney.gov.uk",
                AssignedApprover = "some.otheremail@hackney.gov.uk",
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
                        ParentElementId = null,
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
                        ProviderId = 2,
                        Details = "Some other notes",
                        InternalStatus = ElementStatus.InProgress,
                        ParentElementId = null,
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

            var assignedBroker = new User()
            {
                Name = "UserName",
                Email = "some.email@hackney.gov.uk",
                Roles = new List<UserRole>() {
                    UserRole.BrokerageAssistant
                },
                IsActive = true
            };

            var assignedApprover = new User()
            {
                Name = "Another Username",
                Email = "some.otheremail@hackney.gov.uk",
                Roles = new List<UserRole>() {
                    UserRole.BrokerageAssistant
                },
                IsActive = true
            };

            await Context.Services.AddAsync(service);
            await Context.Services.AddAsync(anotherService);
            await Context.ElementTypes.AddAsync(hourlyElementType);
            await Context.ElementTypes.AddAsync(dailyElementType);
            await Context.Providers.AddAsync(provider);
            await Context.Providers.AddAsync(anotherProvider);

            await Context.ProviderServices.AddAsync(providerService);
            await Context.ProviderServices.AddAsync(anotherProviderService);

            await Context.Referrals.AddAsync(referral);
            await Context.Users.AddAsync(assignedBroker);
            await Context.Users.AddAsync(assignedApprover);
            await Context.SaveChangesAsync();

            Context.ChangeTracker.Clear();

            // Act
            var (code, response) = await Get<List<CarePackageResponse>>($"/api/v1/service-user/{referral.SocialCareId}/care-packages");
            // Assert
            Assert.That(code, Is.EqualTo(HttpStatusCode.OK));

            Assert.That(response[0].Id, Is.EqualTo(referral.Id));
            Assert.That(response[0].StartDate, Is.EqualTo(previousStartDate));
            Assert.That(response[0].WeeklyCost, Is.EqualTo(225));
            Assert.That(response[0].WeeklyPayment, Is.EqualTo(125));
            Assert.That(response[0].Elements.Count, Is.EqualTo(2));
            Assert.That(response[0].CarePackageName, Is.EqualTo("A Different Service, Supported Living"));


            Assert.That(response[0].Elements[0].Status, Is.EqualTo(ElementStatus.Active));
            Assert.That(response[0].Elements[0].Details, Is.EqualTo("Some notes"));
            Assert.That(response[0].Elements[0].ElementType.Name, Is.EqualTo("Day Opportunities (hourly)"));
            Assert.That(response[0].Elements[0].ElementType.Service.Name, Is.EqualTo("Supported Living"));
            Assert.That(response[0].Elements[0].Provider.Name, Is.EqualTo("Acme Homes"));

            Assert.That(response[0].Elements[1].Status, Is.EqualTo(ElementStatus.InProgress));
            Assert.That(response[0].Elements[1].Details, Is.EqualTo("Some other notes"));
            Assert.That(response[0].Elements[1].ElementType.Name, Is.EqualTo("Day Opportunities (daily)"));
            Assert.That(response[0].Elements[1].ElementType.Service.Name, Is.EqualTo("A Different Service"));
            Assert.That(response[0].Elements[1].Provider.Name, Is.EqualTo("Testington Homes"));

            Assert.That(response[0].AssignedBroker.Name, Is.EqualTo("UserName"));
            Assert.That(response[0].AssignedBroker.Email, Is.EqualTo("some.email@hackney.gov.uk"));
            Assert.That(response[0].AssignedApprover.Name, Is.EqualTo("Another Username"));
            Assert.That(response[0].AssignedApprover.Email, Is.EqualTo("some.otheremail@hackney.gov.uk"));
        }
    }
}
