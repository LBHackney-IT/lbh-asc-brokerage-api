using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using BrokerageApi.V1.Gateways;
using BrokerageApi.V1.Infrastructure;
using NUnit.Framework;

namespace BrokerageApi.Tests.V1.Gateways
{
    [TestFixture]
    public class CarePackageGatewayTests : DatabaseTests
    {
        private CarePackageGateway _classUnderTest;

        [SetUp]
        public void Setup()
        {
            _classUnderTest = new CarePackageGateway(BrokerageContext);
        }

        [Test]
        public async Task GetsCarePackageById()
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
                        InternalStatus = ElementStatus.InProgress,
                        ParentElementId = null,
                        StartDate = startDate,
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

            await BrokerageContext.Services.AddAsync(service);
            await BrokerageContext.ElementTypes.AddAsync(hourlyElementType);
            await BrokerageContext.ElementTypes.AddAsync(dailyElementType);
            await BrokerageContext.Providers.AddAsync(provider);
            await BrokerageContext.ProviderServices.AddAsync(providerService);
            await BrokerageContext.Referrals.AddAsync(referral);
            await BrokerageContext.SaveChangesAsync();

            // Act
            var result = await _classUnderTest.GetByIdAsync(referral.Id);

            // Assert
            Assert.That(result.Id, Is.EqualTo(referral.Id));
            Assert.That(result.StartDate, Is.EqualTo(startDate));
            Assert.That(result.WeeklyCost, Is.EqualTo(225));
            Assert.That(result.WeeklyPayment, Is.EqualTo(125));
            Assert.That(result.Elements.Count, Is.EqualTo(2));

            Assert.That(result.Elements[0].Details, Is.EqualTo("Some notes"));
            Assert.That(result.Elements[0].ElementType.Name, Is.EqualTo("Day Opportunities (hourly)"));
            Assert.That(result.Elements[0].ElementType.Service.Name, Is.EqualTo("Supported Living"));
            Assert.That(result.Elements[0].Provider.Name, Is.EqualTo("Acme Homes"));

            Assert.That(result.Elements[1].Details, Is.EqualTo("Some other notes"));
            Assert.That(result.Elements[1].ElementType.Name, Is.EqualTo("Day Opportunities (daily)"));
            Assert.That(result.Elements[1].ElementType.Service.Name, Is.EqualTo("Supported Living"));
            Assert.That(result.Elements[1].Provider.Name, Is.EqualTo("Acme Homes"));
        }

        [Test]
        public async Task GetsCarePackagesByServiceUserId()
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

            var startDate = CurrentDate.PlusDays(1);

            var referral = new Referral()
            {
                WorkflowId = "3e48adb1-0ca2-456c-845a-efcd4eca4554",
                WorkflowType = WorkflowType.Assessment,
                FormName = "Care act assessment",
                SocialCareId = "33556688",
                ResidentName = "A Service User",
                PrimarySupportReason = "Physical Support",
                AssignedBroker = assignedBroker.Email,
                AssignedApprover = assignedApprover.Email,
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
                        InternalStatus = ElementStatus.InProgress,
                        ParentElementId = null,
                        StartDate = startDate,
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

            await BrokerageContext.Services.AddAsync(service);
            await BrokerageContext.Services.AddAsync(anotherService);

            await BrokerageContext.ElementTypes.AddAsync(hourlyElementType);
            await BrokerageContext.ElementTypes.AddAsync(dailyElementType);
            await BrokerageContext.Providers.AddAsync(provider);
            await BrokerageContext.Providers.AddAsync(anotherProvider);

            await BrokerageContext.ProviderServices.AddAsync(providerService);
            await BrokerageContext.ProviderServices.AddAsync(anotherProviderService);

            await BrokerageContext.Referrals.AddAsync(referral);
            await BrokerageContext.Users.AddAsync(assignedBroker);
            await BrokerageContext.Users.AddAsync(assignedApprover);
            await BrokerageContext.SaveChangesAsync();

            // Act
            var result = await _classUnderTest.GetByServiceUserIdAsync(referral.SocialCareId);

            // Assert
            Assert.That(result.ElementAt(0).Id, Is.EqualTo(referral.Id));
            Assert.That(result.ElementAt(0).StartDate, Is.EqualTo(startDate));
            Assert.That(result.ElementAt(0).WeeklyCost, Is.EqualTo(225));
            Assert.That(result.ElementAt(0).WeeklyPayment, Is.EqualTo(125));
            Assert.That(result.ElementAt(0).Elements.Count, Is.EqualTo(2));
            Assert.That(result.ElementAt(0).CarePackageName, Is.EqualTo("A Different Service, Supported Living"));


            Assert.That(result.ElementAt(0).Elements[0].Details, Is.EqualTo("Some notes"));
            Assert.That(result.ElementAt(0).Elements[0].ElementType.Name, Is.EqualTo("Day Opportunities (hourly)"));
            Assert.That(result.ElementAt(0).Elements[0].ElementType.Service.Name, Is.EqualTo("Supported Living"));
            Assert.That(result.ElementAt(0).Elements[0].Provider.Name, Is.EqualTo("Acme Homes"));

            Assert.That(result.ElementAt(0).Elements[1].Details, Is.EqualTo("Some other notes"));
            Assert.That(result.ElementAt(0).Elements[1].ElementType.Name, Is.EqualTo("Day Opportunities (daily)"));
            Assert.That(result.ElementAt(0).Elements[1].ElementType.Service.Name, Is.EqualTo("A Different Service"));
            Assert.That(result.ElementAt(0).Elements[1].Provider.Name, Is.EqualTo("Testington Homes"));

            Assert.That(result.ElementAt(0).AssignedBroker.Name, Is.EqualTo(assignedBroker.Name));
            Assert.That(result.ElementAt(0).AssignedBroker.Email, Is.EqualTo(assignedBroker.Email));
            Assert.That(result.ElementAt(0).AssignedApprover.Name, Is.EqualTo(assignedApprover.Name));
            Assert.That(result.ElementAt(0).AssignedApprover.Email, Is.EqualTo(assignedApprover.Email));


        }

    }
}
