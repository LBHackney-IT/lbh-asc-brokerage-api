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

        [Test]
        public async Task GetsServiceOverviewById()
        {
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
                        StartDate = new LocalDate(2021, 6, 21),
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
                        StartDate = new LocalDate(2022, 4, 1),
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

            await BrokerageContext.Referrals.AddAsync(assessment);
            await BrokerageContext.Referrals.AddAsync(reassessment);
            await BrokerageContext.Referrals.AddAsync(suspension);
            await BrokerageContext.Services.AddAsync(service);
            await BrokerageContext.ServiceUsers.AddAsync(serviceUser);
            await BrokerageContext.Providers.AddAsync(provider);
            await BrokerageContext.ElementTypes.AddAsync(serviceElementType);
            await BrokerageContext.ElementTypes.AddAsync(careChargeElementType);
            await BrokerageContext.ReferralElements.AddRangeAsync(referralElements);
            await BrokerageContext.SaveChangesAsync();

            // Act
            var result = await _classUnderTest.GetBySocialCareIdAndServiceIdAsync("33445566", 1);

            // Assert
            result.Id.Should().Be(1);
            result.Name.Should().Be("Home Care");
            result.StartDate.Should().Be(new LocalDate(2021, 6, 21));
            result.EndDate.Should().BeNull();
            result.WeeklyCost.Should().Be(500.0m);
            result.WeeklyPayment.Should().Be(400.0m);
            result.AnnualCost.Should().Be(26000.0m);
            result.Status.Should().Be(ServiceStatus.Active);
            result.Elements.Count.Should().Be(3);
            result.Elements[0].Status.Should().Be(ElementStatus.Ended);
            result.Elements[1].Status.Should().Be(ElementStatus.Suspended);
            result.Elements[2].Status.Should().Be(ElementStatus.Suspended);
        }
    }
}
