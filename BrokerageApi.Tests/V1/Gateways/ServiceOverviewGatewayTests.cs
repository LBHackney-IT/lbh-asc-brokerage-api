using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BrokerageApi.V1.Gateways;
using BrokerageApi.V1.Infrastructure;
using FluentAssertions;
using NodaTime;
using NUnit.Framework;

namespace BrokerageApi.Tests.V1.Gateways
{
    [TestFixture]
    public class ServiceOverviewGatewayTests : DatabaseTests
    {
        private ServiceOverviewGateway _classUnderTest;

        [SetUp]
        public void Setup()
        {
            _classUnderTest = new ServiceOverviewGateway(BrokerageContext);
        }

        [Test]
        public async Task GetsServiceOverviews()
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

            await BrokerageContext.Services.AddAsync(service);
            await BrokerageContext.ServiceUsers.AddAsync(serviceUser);
            await BrokerageContext.Providers.AddAsync(provider);
            await BrokerageContext.ElementTypes.AddAsync(serviceElementType);
            await BrokerageContext.ElementTypes.AddAsync(careChargeElementType);
            await BrokerageContext.SaveChangesAsync();

            // Act
            var result = await _classUnderTest.GetBySocialCareIdAsync("33445566");
            var serviceOverviews = result.ToList();

            // Assert
            serviceOverviews.Count.Should().Be(1);
            serviceOverviews[0].Id.Should().Be(1);
            serviceOverviews[0].Name.Should().Be("Home Care");
            serviceOverviews[0].StartDate.Should().Be(new LocalDate(2021, 6, 21));
            serviceOverviews[0].EndDate.Should().BeNull();
            serviceOverviews[0].WeeklyCost.Should().Be(500.0m);
            serviceOverviews[0].WeeklyPayment.Should().Be(400.0m);
            serviceOverviews[0].AnnualCost.Should().Be(26000.0m);
            serviceOverviews[0].Status.Should().Be(ServiceStatus.Active);
        }
    }
}
