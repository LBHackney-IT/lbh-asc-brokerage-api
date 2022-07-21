using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using BrokerageApi.V1.Boundary.Request;
using BrokerageApi.V1.Boundary.Response;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.Tests.V1.Helpers;
using NUnit.Framework;
using AutoFixture;
using NodaTime;


namespace BrokerageApi.Tests.V1.E2ETests
{
    public class ServiceUserTests : IntegrationTests<Startup>
    {

        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _fixture = FixtureHelpers.Fixture;
        }

        [Test, Property("AsUser", "Broker")]
        public async Task CanGetServiceOverviews()
        {
            // Arrange
            var service = new Service()
            {
                Id = 1,
                Name = "Home Care",
                Position = 1,
                IsArchived = false
            };

            var serviceUser = new ServiceUser()
            {
                SocialCareId = "33445566",
                DateOfBirth = new LocalDate(1968, 10, 31),
                ServiceUserName = "Joey Tribiani"
            };

            var provider = new Provider()
            {
                Id = 1,
                Name = "Central Perk",
                Address = "Central Park, New York, NY",
                CedarNumber = "123456",
                CedarSite = "0",
                Type = ProviderType.Spot,
                IsArchived = false
            };

            var serviceElementType = new ElementType()
            {
                Id = 1,
                ServiceId = service.Id,
                Type = ElementTypeType.Service,
                Name = "Personal Home Care (weekly)",
                SubjectiveCode = "520050",
                FrameworkSubjectiveCode = "520061",
                CostType = ElementCostType.Weekly,
                Billing = ElementBillingType.Supplier,
                CostOperation = MathOperation.Add,
                PaymentOperation = MathOperation.Add,
                NonPersonalBudget = false,
                Position = 1,
                IsArchived = false,
                Elements = new List<Element>()
                {
                    new Element()
                    {
                        Id = 20210621,
                        SocialCareId = "33445566",
                        ProviderId = 1,
                        Cost = 450.0m,
                        Quantity = 1.0m,
                        StartDate = new LocalDate(2021, 6,21),
                        EndDate = new LocalDate(2022, 3, 31),
                        InternalStatus = ElementStatus.Approved
                    },
                    new Element()
                    {
                        Id = 20220401,
                        SocialCareId = "33445566",
                        ProviderId = 1,
                        Cost = 500.0m,
                        Quantity = 1.0m,
                        StartDate = new LocalDate(2022, 4,1),
                        EndDate = null,
                        InternalStatus = ElementStatus.Approved
                    }
                }
            };

            var careChargeElementType = new ElementType()
            {
                Id = 2,
                ServiceId = service.Id,
                Type = ElementTypeType.ConfirmedCareCharge,
                Name = "Non-Residential Care Charges (collected by Hackney)",
                SubjectiveCode = "920128",
                FrameworkSubjectiveCode = null,
                CostType = ElementCostType.Weekly,
                Billing = ElementBillingType.Customer,
                CostOperation = MathOperation.Ignore,
                PaymentOperation = MathOperation.Subtract,
                NonPersonalBudget = false,
                Position = 2,
                IsArchived = false,
                Elements = new List<Element>()
                {
                    new Element()
                    {
                        Id = 20210721,
                        SocialCareId = "33445566",
                        Cost = 100.0m,
                        Quantity = 1.0m,
                        StartDate = new LocalDate(2021, 7, 21),
                        EndDate = null,
                        InternalStatus = ElementStatus.Approved
                    }
                }
            };

            await Context.Services.AddAsync(service);
            await Context.ServiceUsers.AddAsync(serviceUser);
            await Context.Providers.AddAsync(provider);
            await Context.ElementTypes.AddAsync(serviceElementType);
            await Context.ElementTypes.AddAsync(careChargeElementType);
            await Context.SaveChangesAsync();

            Context.ChangeTracker.Clear();

            // Act
            var (code, response) = await Get<List<ServiceOverviewResponse>>($"/api/v1/service-users/33445566/services");

            // Assert
            Assert.That(code, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Count, Is.EqualTo(1));
            Assert.That(response[0].Id, Is.EqualTo(1));
            Assert.That(response[0].Name, Is.EqualTo("Home Care"));
            Assert.That(response[0].StartDate, Is.EqualTo(new LocalDate(2021, 6, 21)));
            Assert.That(response[0].EndDate, Is.Null);
            Assert.That(response[0].WeeklyCost, Is.EqualTo(500.0m));
            Assert.That(response[0].WeeklyPayment, Is.EqualTo(400.0m));
            Assert.That(response[0].AnnualCost, Is.EqualTo(26000.0m));
            Assert.That(response[0].Status, Is.EqualTo(ServiceStatus.Active));
        }

        [Test, Property("AsUser", "Broker")]
        public async Task CanGetServiceOverviewById()
        {
            // Arrange
            var assessment = new Referral()
            {
                Id = 1,
                WorkflowId = "4f61bee0-e909-013a-99fa-5a4fae5edecc",
                WorkflowType = WorkflowType.Assessment,
                SocialCareId = "33445566",
                ResidentName = "Joey Tribiani",
                FormName = "Care act assessment",
                Status = ReferralStatus.Approved
            };

            var reassessment = new Referral()
            {
                Id = 2,
                WorkflowId = "2af1a090-e90a-013a-99fb-5a4fae5edecc",
                WorkflowType = WorkflowType.Reassessment,
                SocialCareId = "33445566",
                ResidentName = "Joey Tribiani",
                FormName = "Care act assessment",
                Status = ReferralStatus.Approved
            };

            var suspension = new Referral()
            {
                Id = 3,
                WorkflowId = "a543fa70-e91a-013a-99fc-5a4fae5edecc",
                WorkflowType = WorkflowType.Reassessment,
                SocialCareId = "33445566",
                ResidentName = "Joey Tribiani",
                FormName = "Care act assessment",
                Status = ReferralStatus.Approved
            };

            var service = new Service()
            {
                Id = 1,
                Name = "Home Care",
                Position = 1,
                IsArchived = false
            };

            var serviceUser = new ServiceUser()
            {
                SocialCareId = "33445566",
                DateOfBirth = new LocalDate(1968, 10, 31),
                ServiceUserName = "Joey Tribiani"
            };

            var provider = new Provider()
            {
                Id = 1,
                Name = "Central Perk",
                Address = "Central Park, New York, NY",
                CedarNumber = "123456",
                CedarSite = "0",
                Type = ProviderType.Spot,
                IsArchived = false
            };

            var serviceElementType = new ElementType()
            {
                Id = 1,
                ServiceId = service.Id,
                Type = ElementTypeType.Service,
                Name = "Personal Home Care (weekly)",
                SubjectiveCode = "520050",
                FrameworkSubjectiveCode = "520061",
                CostType = ElementCostType.Weekly,
                Billing = ElementBillingType.Supplier,
                CostOperation = MathOperation.Add,
                PaymentOperation = MathOperation.Add,
                NonPersonalBudget = false,
                Position = 1,
                IsArchived = false,
                Elements = new List<Element>()
                {
                    new Element()
                    {
                        Id = 20210621,
                        SocialCareId = "33445566",
                        ProviderId = 1,
                        Cost = 450.0m,
                        Quantity = 1.0m,
                        StartDate = new LocalDate(2021, 6,21),
                        EndDate = new LocalDate(2022, 3, 31),
                        InternalStatus = ElementStatus.Approved
                    },
                    new Element()
                    {
                        Id = 20220401,
                        SocialCareId = "33445566",
                        ProviderId = 1,
                        Cost = 500.0m,
                        Quantity = 1.0m,
                        StartDate = new LocalDate(2022, 4,1),
                        EndDate = null,
                        InternalStatus = ElementStatus.Approved
                    },
                    new Element()
                    {
                        Id = 20220601,
                        SocialCareId = "33445566",
                        ProviderId = 1,
                        Cost = 500.0m,
                        Quantity = 1.0m,
                        StartDate = new LocalDate(2022, 6, 1),
                        EndDate = null,
                        InternalStatus = ElementStatus.Approved,
                        SuspendedElementId = 20220401,
                        IsSuspension = true
                    }
                }
            };

            var careChargeElementType = new ElementType()
            {
                Id = 2,
                ServiceId = service.Id,
                Type = ElementTypeType.ConfirmedCareCharge,
                Name = "Non-Residential Care Charges (collected by Hackney)",
                SubjectiveCode = "920128",
                FrameworkSubjectiveCode = null,
                CostType = ElementCostType.Weekly,
                Billing = ElementBillingType.Customer,
                CostOperation = MathOperation.Ignore,
                PaymentOperation = MathOperation.Subtract,
                NonPersonalBudget = false,
                Position = 2,
                IsArchived = false,
                Elements = new List<Element>()
                {
                    new Element()
                    {
                        Id = 20210721,
                        SocialCareId = "33445566",
                        Cost = 100.0m,
                        Quantity = 1.0m,
                        StartDate = new LocalDate(2021, 7, 21),
                        EndDate = null,
                        InternalStatus = ElementStatus.Approved
                    },
                    new Element()
                    {
                        Id = 20220602,
                        SocialCareId = "33445566",
                        Cost = 100.0m,
                        Quantity = 1.0m,
                        StartDate = new LocalDate(2022, 6, 2),
                        EndDate = null,
                        InternalStatus = ElementStatus.Approved,
                        SuspendedElementId = 20210721,
                        IsSuspension = true
                    }
                }
            };

            var referralElements = new List<ReferralElement>()
            {
                new ReferralElement() { ReferralId = 1, ElementId = 20210621 },
                new ReferralElement() { ReferralId = 2, ElementId = 20220401 },
                new ReferralElement() { ReferralId = 1, ElementId = 20210721 },
                new ReferralElement() { ReferralId = 3, ElementId = 20220601 },
                new ReferralElement() { ReferralId = 3, ElementId = 20220602 }
            };

            await Context.Referrals.AddAsync(assessment);
            await Context.Referrals.AddAsync(reassessment);
            await Context.Referrals.AddAsync(suspension);
            await Context.Services.AddAsync(service);
            await Context.ServiceUsers.AddAsync(serviceUser);
            await Context.Providers.AddAsync(provider);
            await Context.ElementTypes.AddAsync(serviceElementType);
            await Context.ElementTypes.AddAsync(careChargeElementType);
            await Context.ReferralElements.AddRangeAsync(referralElements);
            await Context.SaveChangesAsync();

            // Act
            var (code, response) = await Get<ServiceOverviewResponse>($"/api/v1/service-users/33445566/services/1");

            // Assert
            Assert.That(code, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Id, Is.EqualTo(1));
            Assert.That(response.Name, Is.EqualTo("Home Care"));
            Assert.That(response.StartDate, Is.EqualTo(new LocalDate(2021, 6, 21)));
            Assert.That(response.EndDate, Is.Null);
            Assert.That(response.WeeklyCost, Is.EqualTo(500.0m));
            Assert.That(response.WeeklyPayment, Is.EqualTo(400.0m));
            Assert.That(response.AnnualCost, Is.EqualTo(26000.0m));
            Assert.That(response.Status, Is.EqualTo(ServiceStatus.Active));
            Assert.That(response.Elements.Count, Is.EqualTo(3));
            Assert.That(response.Elements[0].Type, Is.EqualTo(ElementTypeType.Service));
            Assert.That(response.Elements[0].Status, Is.EqualTo(ElementStatus.Ended));
            Assert.That(response.Elements[1].Type, Is.EqualTo(ElementTypeType.ConfirmedCareCharge));
            Assert.That(response.Elements[1].Status, Is.EqualTo(ElementStatus.Suspended));
            Assert.That(response.Elements[2].Type, Is.EqualTo(ElementTypeType.Service));
            Assert.That(response.Elements[2].Status, Is.EqualTo(ElementStatus.Suspended));
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
                CostOperation = MathOperation.Add,
                PaymentOperation = MathOperation.Add,
                NonPersonalBudget = false,
                IsArchived = false
            };

            var dailyElementType = new ElementType
            {
                Id = 2,
                ServiceId = 2,
                Name = "Day Opportunities (daily)",
                CostType = ElementCostType.Daily,
                CostOperation = MathOperation.Add,
                PaymentOperation = MathOperation.Add,
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
                AssignedBrokerEmail = "some.email@hackney.gov.uk",
                AssignedApproverEmail = "some.otheremail@hackney.gov.uk",
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
                        Wednesday = new ElementCost(1, 100),
                        Quantity = 1,
                        Cost = 100,
                        CreatedAt = CurrentInstant,
                        UpdatedAt = CurrentInstant
                    },
                }
            };

            var assignedBrokerEmail = new User()
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

            await Context.Referrals.AddAsync(referral);
            await Context.Users.AddAsync(assignedBrokerEmail);
            await Context.Users.AddAsync(assignedApprover);
            await Context.SaveChangesAsync();

            Context.ChangeTracker.Clear();

            // Act
            var (code, response) = await Get<List<CarePackageResponse>>($"/api/v1/service-users/{referral.SocialCareId}/care-packages");

            // Assert
            Assert.That(code, Is.EqualTo(HttpStatusCode.OK));

            Assert.That(response[0].Id, Is.EqualTo(referral.Id));
            Assert.That(response[0].StartDate, Is.EqualTo(previousStartDate));
            Assert.That(response[0].WeeklyCost, Is.EqualTo(325));
            Assert.That(response[0].WeeklyPayment, Is.EqualTo(325));
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

        [Test, Property("AsUser", "Broker")]
        public async Task CanGetServiceUsersByRequest()
        {
            //Arrange
            var serviceUser = _fixture.BuildServiceUser().Create();
            await Context.ServiceUsers.AddAsync(serviceUser);

            await Context.SaveChangesAsync();
            Context.ChangeTracker.Clear();
            //Act
            var (code, response) = await Get<List<ServiceUserResponse>>($"/api/v1/service-users/?SocialCareId={serviceUser.SocialCareId}");
            // Assert
            Assert.That(response, Has.Count.EqualTo(1));
            Assert.That(code, Is.EqualTo(HttpStatusCode.OK));

        }
        [Test, Property("AsUser", "CareChargesOfficer")]
        public async Task CanEditServiceUserCedarNumber()
        {
            //Arrange
            var serviceUser = _fixture.BuildServiceUser().Create();
            await Context.ServiceUsers.AddAsync(serviceUser);

            await Context.SaveChangesAsync();
            Context.ChangeTracker.Clear();
            var request = _fixture.Build<EditServiceUserRequest>()
                .With(su => su.SocialCareId, serviceUser.SocialCareId)
                .With(su => su.CedarNumber, "aCedarNumber")
                .Create();
            //Act
            var (code, response) = await Post<ServiceUserResponse>($"/api/v1/service-users/cedar-number", request);
            // Assert
            Assert.That(code, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.CedarNumber, Is.EqualTo("aCedarNumber"));

        }
    }
}
