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
    }
}
