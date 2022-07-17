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
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

            var previousStartDate = CurrentDate.PlusDays(-100);
            var startDate = CurrentDate.PlusDays(1);

            var parentElement = _fixture.BuildElement(hourlyElementType.Id, provider.Id)
                .Create();

            var elementWithSuspensions = _fixture.BuildElement(hourlyElementType.Id, provider.Id)
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
                Id = 111,
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
                Id = 222,
                SocialCareId = "33556688",
                ElementTypeId = dailyElementType.Id,
                NonPersonalBudget = false,
                ProviderId = provider.Id,
                Details = "Some other notes",
                InternalStatus = ElementStatus.InProgress,
                ParentElementId = null,
                StartDate = startDate,
                EndDate = null,
                Wednesday = new ElementCost(1, 100),
                Quantity = 1,
                Cost = 100,
                CreatedAt = CurrentInstant,
                UpdatedAt = CurrentInstant,
                ParentElement = parentElement
            };

            var workflow = new Workflow()
            {
                Id = "3e48adb1-0ca2-456c-845a-efcd4eca4554",
                WorkflowType = WorkflowType.Assessment,
                FormName = "Care act assessment",
                PrimarySupportReason = "Physical Support"
            };

            var referral = new Referral()
            {
                Id = 1234,
                WorkflowId = "3e48adb1-0ca2-456c-845a-efcd4eca4554",
                WorkflowType = WorkflowType.Assessment,
                FormName = "Care act assessment",
                SocialCareId = "33556688",
                ResidentName = "A Service User",
                PrimarySupportReason = "Physical Support",
                AssignedBrokerEmail = ApiUser.Email,
                Status = ReferralStatus.InProgress,
                StartedAt = CurrentInstant,
                CreatedAt = PreviousInstant,
                UpdatedAt = CurrentInstant,
                Workflows = new List<Workflow> { workflow }
            };

            var referralElements = new List<ReferralElement>
            {
                _fixture.BuildReferralElement(referral.Id, element1.Id).Create(),
                _fixture.BuildReferralElement(referral.Id, element2.Id).Create(),
                _fixture.BuildReferralElement(referral.Id, elementWithSuspensions.Id).Create()
            };

            await Context.Services.AddAsync(service);
            await Context.ElementTypes.AddAsync(hourlyElementType);
            await Context.ElementTypes.AddAsync(dailyElementType);
            await Context.Elements.AddAsync(parentElement);
            await Context.Elements.AddRangeAsync(suspensions);
            await Context.Providers.AddAsync(provider);
            await Context.Referrals.AddAsync(referral);
            await Context.Elements.AddRangeAsync(new List<Element>
            {
                element1,
                element2,
                elementWithSuspensions
            });
            await Context.ReferralElements.AddRangeAsync(referralElements);
            await Context.SaveChangesAsync();

            Context.ChangeTracker.Clear();

            // Act
            var (code, response) = await Get<CarePackageResponse>($"/api/v1/referrals/{referral.Id}/care-package");

            // Assert
            code.Should().Be(HttpStatusCode.OK);

            response.Id.Should().Be(referral.Id);
            response.StartDate.Should().Be(previousStartDate);
            response.WeeklyCost.Should().Be(325);
            response.WeeklyPayment.Should().Be(325);
            response.Workflows[0].Should().BeEquivalentTo(workflow.ToResponse());
            response.Elements.Should().HaveCount(3);

            var responseElement1 = response.Elements.Single(e => e.Id == element1.Id);
            ValidateElementResponse(responseElement1, element1, hourlyElementType, service, provider, parentElement, referral.Id);
            responseElement1.Status.Should().Be(ElementStatus.Active);

            var responseElement2 = response.Elements.Single(e => e.Id == element2.Id);
            ValidateElementResponse(responseElement2, element2, dailyElementType, service, provider, parentElement, referral.Id);
            responseElement2.Status.Should().Be(ElementStatus.InProgress);

            var responseElement3 = response.Elements.Single(e => e.Id == elementWithSuspensions.Id);
            ValidateElementResponse(responseElement3, elementWithSuspensions, hourlyElementType, service, provider, null, referral.Id);
            responseElement3.SuspensionElements.Should().BeEquivalentTo(suspensions.Select(e => e.ToResponse()));
        }

        private static void ValidateElementResponse(ElementResponse elementResponse,
            Element element,
            ElementType elementType,
            Service service,
            Provider provider,
            Element parentElement,
            int referralId)
        {
            elementResponse.Details.Should().Be(element.Details);
            elementResponse.ElementType.Name.Should().Be(elementType.Name);
            elementResponse.ElementType.Service.Name.Should().Be(service.Name);
            elementResponse.Provider.Name.Should().Be(provider.Name);
            if (parentElement != null)
            {
                elementResponse.ParentElement.Should().BeEquivalentTo(parentElement.ToResponse());
            }

            var referralElement = element.ReferralElements.Single(re => re.ReferralId == referralId);
            elementResponse.PendingEndDate.Should().Be(referralElement.PendingEndDate);
            elementResponse.PendingCancellation.Should().Be(referralElement.PendingCancellation);
            elementResponse.PendingComment.Should().Be(referralElement.PendingComment);
        }

        [Test, Property("AsUser", "Broker")]
        public async Task CanStartCarePackage()
        {
            // Arrange
            var service = _fixture.BuildService()
                .Create();

            var elementType = _fixture.BuildElementType(service.Id)
                .With(et => et.CostType, ElementCostType.Hourly)
                .With(et => et.NonPersonalBudget, false)
                .Create();

            var provider = _fixture.BuildProvider()
                .Create();

            var startDate = CurrentDate.PlusDays(-100);

            var parentElement = _fixture.BuildElement(elementType.Id, provider.Id)
                .Create();

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
                AssignedBrokerEmail = "api.user@hackney.gov.uk",
                StartedAt = null,
                CreatedAt = PreviousInstant,
                UpdatedAt = PreviousInstant
            };

            var element = new Element
            {
                Id = 111,
                SocialCareId = "33556688",
                ElementTypeId = elementType.Id,
                NonPersonalBudget = false,
                ProviderId = provider.Id,
                Details = "Some notes",
                InternalStatus = ElementStatus.Approved,
                ParentElementId = null,
                StartDate = startDate,
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

            await Context.Services.AddAsync(service);
            await Context.ElementTypes.AddAsync(elementType);
            await Context.Providers.AddAsync(provider);
            await Context.Elements.AddAsync(element);
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

            var referralElements = await Context.ReferralElements.Where(re => re.ReferralId == referral.Id).ToListAsync();
            referralElements.Count.Should().Be(1);
        }

        [Test, Property("AsUser", "ReferrerAndBroker")]
        public async Task CanStartCarePackageWhenUserIsAReferrerAndABroker()
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
                AssignedBrokerEmail = "api.user@hackney.gov.uk",
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
        public async Task CantStartCarePackageWhenAnotherIsInProgress()
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
                AssignedBrokerEmail = "api.user@hackney.gov.uk",
                StartedAt = null,
                CreatedAt = CurrentInstant,
                UpdatedAt = CurrentInstant
            };

            var otherReferral = new Referral()
            {
                WorkflowId = "c827cb90-e5a8-013a-99f8-5a4fae5edecc",
                WorkflowType = WorkflowType.Assessment,
                FormName = "Care act assessment",
                SocialCareId = "33556688",
                ResidentName = "A Service User",
                PrimarySupportReason = "Physical Support",
                DirectPayments = "No",
                Status = ReferralStatus.InProgress,
                AssignedBrokerEmail = "api.user@hackney.gov.uk",
                StartedAt = PreviousInstant,
                CreatedAt = PreviousInstant,
                UpdatedAt = PreviousInstant
            };

            await Context.Referrals.AddAsync(referral);
            await Context.Referrals.AddAsync(otherReferral);
            await Context.SaveChangesAsync();

            Context.ChangeTracker.Clear();

            // Act
            var (code, response) = await Post<ProblemDetails>($"/api/v1/referrals/{referral.Id}/care-package/start", null);

            // Assert
            code.Should().Be(HttpStatusCode.UnprocessableEntity);
            response.Status.Should().Be((int) HttpStatusCode.UnprocessableEntity);
            response.Detail.Should().Be("A referral for A Service User is already in progress or awaiting approval");
        }

        [Test, Property("AsUser", "Broker")]
        public async Task CantStartCarePackageWhenAnotherIsAwaitingApproval()
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
                AssignedBrokerEmail = "api.user@hackney.gov.uk",
                StartedAt = null,
                CreatedAt = CurrentInstant,
                UpdatedAt = CurrentInstant
            };

            var otherReferral = new Referral()
            {
                WorkflowId = "c827cb90-e5a8-013a-99f8-5a4fae5edecc",
                WorkflowType = WorkflowType.Assessment,
                FormName = "Care act assessment",
                SocialCareId = "33556688",
                ResidentName = "A Service User",
                PrimarySupportReason = "Physical Support",
                DirectPayments = "No",
                Status = ReferralStatus.AwaitingApproval,
                AssignedBrokerEmail = "api.user@hackney.gov.uk",
                StartedAt = PreviousInstant,
                CreatedAt = PreviousInstant,
                UpdatedAt = PreviousInstant
            };

            await Context.Referrals.AddAsync(referral);
            await Context.Referrals.AddAsync(otherReferral);
            await Context.SaveChangesAsync();

            Context.ChangeTracker.Clear();

            // Act
            var (code, response) = await Post<ProblemDetails>($"/api/v1/referrals/{referral.Id}/care-package/start", null);

            // Assert
            code.Should().Be(HttpStatusCode.UnprocessableEntity);
            response.Status.Should().Be((int) HttpStatusCode.UnprocessableEntity);
            response.Detail.Should().Be("A referral for A Service User is already in progress or awaiting approval");
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

            var elementType = _fixture.BuildElementType(service.Id)
                .Create();

            var elements = _fixture.BuildElement(elementType.Id, provider.Id)
                .With(e => e.InternalStatus, ElementStatus.Approved)
                .With(e => e.StartDate, endDate.PlusDays(-5))
                .With(e => e.EndDate, endDate.PlusDays(5))
                .CreateMany();

            var referral = _fixture.BuildReferral(ReferralStatus.Approved)
                .With(r => r.AssignedBrokerEmail, ApiUser.Email)
                .With(r => r.Elements, elements.ToList)
                .Create();

            await Context.Referrals.AddAsync(referral);
            await Context.Services.AddAsync(service);
            await Context.Providers.AddAsync(provider);
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
            response.UpdatedAt.Should().BeEquivalentTo(CurrentInstant);

            response.Elements.Should().OnlyContain(e => e.PendingEndDate == endDate);
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

            var elementType = _fixture.BuildElementType(service.Id)
                .Create();

            var elements = _fixture.BuildElement(elementType.Id, provider.Id)
                .With(e => e.InternalStatus, ElementStatus.Approved)
                .CreateMany();

            var referral = _fixture.BuildReferral(ReferralStatus.Approved)
                .With(r => r.AssignedBrokerEmail, ApiUser.Email)
                .With(r => r.Elements, elements.ToList)
                .Create();

            await Context.Referrals.AddAsync(referral);
            await Context.Services.AddAsync(service);
            await Context.Providers.AddAsync(provider);
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

            response.Elements.Should().OnlyContain(e => e.PendingCancellation == true);
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

            var elementType = _fixture.BuildElementType(service.Id)
                .Create();

            var elements = _fixture.BuildElement(elementType.Id, provider.Id)
                .With(e => e.InternalStatus, ElementStatus.Approved)
                .With(e => e.StartDate, startDate.PlusDays(-5))
                .With(e => e.EndDate, endDate.PlusDays(5))
                .CreateMany();

            var referral = _fixture.BuildReferral(ReferralStatus.Approved)
                .With(r => r.AssignedBrokerEmail, ApiUser.Email)
                .With(r => r.Elements, elements.ToList)
                .Create();

            await Context.Referrals.AddAsync(referral);
            await Context.Services.AddAsync(service);
            await Context.Providers.AddAsync(provider);
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

        [Test, Property("AsUser", "Broker")]
        public async Task CanSuspendCarePackageWithoutEndDate()
        {
            // Arrange
            var startDate = CurrentDate;
            var service = _fixture.BuildService()
                .Create();

            var provider = _fixture.BuildProvider()
                .Create();

            var elementType = _fixture.BuildElementType(service.Id)
                .Create();

            var elements = _fixture.BuildElement(elementType.Id, provider.Id)
                .With(e => e.InternalStatus, ElementStatus.Approved)
                .With(e => e.StartDate, startDate.PlusDays(-5))
                .With(e => e.EndDate, startDate.PlusDays(5))
                .CreateMany();

            var referral = _fixture.BuildReferral(ReferralStatus.Approved)
                .With(r => r.AssignedBrokerEmail, ApiUser.Email)
                .With(r => r.Elements, elements.ToList)
                .Create();

            await Context.Referrals.AddAsync(referral);
            await Context.Services.AddAsync(service);
            await Context.Providers.AddAsync(provider);
            await Context.ElementTypes.AddAsync(elementType);
            await Context.SaveChangesAsync();

            Context.ChangeTracker.Clear();

            var request = _fixture.Build<SuspendRequest>()
                .With(r => r.StartDate, startDate)
                .Without(r => r.EndDate)
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

        [Test, Property("AsUser", "Broker")]
        public async Task CanGetBudgetApprovers()
        {
            // Arrange
            var service = _fixture.BuildService()
                .Create();

            var provider = _fixture.BuildProvider()
                .Create();

            var dailyElementType = _fixture.BuildElementType(service.Id)
                .With(et => et.CostType, ElementCostType.Daily)
                .Create();

            var oneOffElementType = _fixture.BuildElementType(service.Id)
                .With(et => et.CostType, ElementCostType.OneOff)
                .With(et => et.CostOperation, MathOperation.Ignore)
                .With(et => et.PaymentOperation, MathOperation.Ignore)
                .Create();

            var dailyElements = _fixture.BuildElement(dailyElementType.Id, provider.Id)
                .With(e => e.InternalStatus, ElementStatus.Approved)
                .CreateMany();

            var oneOffElements = _fixture.BuildElement(oneOffElementType.Id, provider.Id)
                .With(e => e.InternalStatus, ElementStatus.Approved)
                .CreateMany();

            var referral = _fixture.BuildReferral(ReferralStatus.InProgress)
                .With(r => r.AssignedBrokerEmail, ApiUser.Email)
                .With(r => r.Elements, dailyElements.Concat(oneOffElements).ToList)
                .Create();

            var expectedYearlyCost =
                dailyElements.Sum(e => e.Cost) * 52 +
                oneOffElements.Sum(e => e.Cost);

            var expectedApprovers = _fixture.BuildUser()
                .With(u => u.IsActive, true)
                .With(u => u.Roles, new List<UserRole>
                {
                    UserRole.Approver
                })
                .With(u => u.ApprovalLimit, expectedYearlyCost + 100)
                .CreateMany();

            var notExpectedApprovers = _fixture.BuildUser()
                .With(u => u.IsActive, true)
                .With(u => u.Roles, new List<UserRole>
                {
                    UserRole.Approver
                })
                .With(u => u.ApprovalLimit, expectedYearlyCost - 100)
                .CreateMany();

            await Context.Users.AddRangeAsync(expectedApprovers);
            await Context.Users.AddRangeAsync(notExpectedApprovers);
            await Context.Referrals.AddAsync(referral);
            await Context.Services.AddAsync(service);
            await Context.Providers.AddAsync(provider);
            await Context.ElementTypes.AddAsync(dailyElementType);
            await Context.ElementTypes.AddAsync(oneOffElementType);
            await Context.SaveChangesAsync();

            Context.ChangeTracker.Clear();

            var (code, response) = await Get<GetApproversResponse>($"/api/v1/referrals/{referral.Id}/care-package/budget-approvers");

            code.Should().Be(HttpStatusCode.OK);
            response.EstimatedYearlyCost.Should().Be(expectedYearlyCost);
            response.Approvers.Should().BeEquivalentTo(expectedApprovers.Select(u => u.ToResponse()));
        }

        [Test, Property("AsUser", "Broker")]
        public async Task CanAssignBudgetApprover()
        {
            // Arrange
            var amendments = _fixture.BuildReferralAmendment()
                .With(a => a.Status, AmendmentStatus.InProgress)
                .CreateMany();

            var referral = _fixture.BuildReferral(ReferralStatus.InProgress)
                .With(r => r.AssignedBrokerEmail, ApiUser.Email)
                .With(r => r.ReferralAmendments, amendments.ToList)
                .Create();

            var expectedApprover = _fixture.BuildUser()
                .With(u => u.IsActive, true)
                .With(u => u.Roles, new List<UserRole>
                {
                    UserRole.Approver
                })
                .With(u => u.ApprovalLimit, 100)
                .Create();

            await Context.Users.AddAsync(expectedApprover);
            await Context.Referrals.AddAsync(referral);
            await Context.SaveChangesAsync();

            Context.ChangeTracker.Clear();

            var request = _fixture.Build<AssignApproverRequest>()
                .With(r => r.Approver, expectedApprover.Email)
                .Create();

            var code = await Post($"/api/v1/referrals/{referral.Id}/care-package/assign-budget-approver", request);

            code.Should().Be(HttpStatusCode.OK);

            var (referralCode, response) = await Get<ReferralResponse>($"/api/v1/referrals/{referral.Id}");

            referralCode.Should().Be(HttpStatusCode.OK);
            response.AssignedApprover.Should().BeEquivalentTo(expectedApprover.ToResponse());
            response.Amendments.Should().OnlyContain(a => a.Status == AmendmentStatus.Resolved);
        }

        [Test, Property("AsUser", "Approver"), Property("WithApprovalLimit", 1000)]
        public async Task CanApproveCarePackage()
        {
            // Arrange
            var service = _fixture.BuildService()
                .Create();

            var provider = _fixture.BuildProvider()
                .Create();

            var oneOffElementType = _fixture.BuildElementType(service.Id)
                .With(et => et.CostType, ElementCostType.OneOff)
                .With(et => et.CostOperation, MathOperation.Ignore)
                .With(et => et.PaymentOperation, MathOperation.Ignore)
                .Create();

            var referral = _fixture.BuildReferral(ReferralStatus.AwaitingApproval)
                .With(r => r.AssignedBrokerEmail, ApiUser.Email)
                .Create();

            var oldReferral = _fixture.BuildReferral(ReferralStatus.Approved)
                .With(r => r.SocialCareId, referral.SocialCareId)
                .Create();

            var newElement = _fixture.BuildElement(oneOffElementType.Id, provider.Id)
                .With(e => e.InternalStatus, ElementStatus.AwaitingApproval)
                .With(e => e.Cost, 100)
                .Create();
            var newReferralElement = _fixture.BuildReferralElement(referral.Id, newElement.Id)
                .Create();

            var parentElement = _fixture.BuildElement(oneOffElementType.Id, provider.Id)
                .With(e => e.InternalStatus, ElementStatus.Approved)
                .With(e => e.Cost, 100)
                .Without(e => e.EndDate)
                .Create();
            var parentReferralElement = _fixture.BuildReferralElement(referral.Id, parentElement.Id)
                .Create();

            var childElement = _fixture.BuildElement(oneOffElementType.Id, provider.Id)
                .With(e => e.InternalStatus, ElementStatus.AwaitingApproval)
                .With(e => e.Cost, 100)
                .With(e => e.ParentElement, parentElement)
                .Create();
            var childReferralElement = _fixture.BuildReferralElement(referral.Id, childElement.Id)
                .Create();

            var cancelElement = _fixture.BuildElement(oneOffElementType.Id, provider.Id)
                .With(e => e.InternalStatus, ElementStatus.Approved)
                .Create();
            var cancelReferralElement = _fixture.BuildReferralElement(referral.Id, cancelElement.Id)
                .With(re => re.PendingCancellation, true)
                .With(re => re.PendingComment, "comment here")
                .Create();

            var endElement = _fixture.BuildElement(oneOffElementType.Id, provider.Id)
                .With(e => e.InternalStatus, ElementStatus.Approved)
                .Without(e => e.EndDate)
                .Create();
            var endReferralElement = _fixture.BuildReferralElement(referral.Id, endElement.Id)
                .With(re => re.PendingEndDate, CurrentDate)
                .Create();

            await Context.Referrals.AddAsync(referral);
            await Context.Referrals.AddAsync(oldReferral);
            await Context.Services.AddAsync(service);
            await Context.Providers.AddAsync(provider);
            await Context.ElementTypes.AddAsync(oneOffElementType);
            await Context.Referrals.AddAsync(referral);
            await Context.Elements.AddRangeAsync(newElement, parentElement, childElement, cancelElement, endElement);
            await Context.ReferralElements.AddRangeAsync(newReferralElement, parentReferralElement, childReferralElement, cancelReferralElement, endReferralElement);
            await Context.SaveChangesAsync();

            Context.ChangeTracker.Clear();

            var code = await Post($"/api/v1/referrals/{referral.Id}/care-package/approve", null);

            code.Should().Be(HttpStatusCode.OK);
            var resultReferral = Context.Referrals.Single(r => r.Id == referral.Id);
            resultReferral.Status.Should().Be(ReferralStatus.Approved);

            var newElementResult = await Context.Elements.SingleAsync(e => e.Id == newElement.Id);
            newElementResult.InternalStatus.Should().Be(ElementStatus.Approved);

            var childElementResult = await Context.Elements.SingleAsync(e => e.Id == childElement.Id);
            childElementResult.InternalStatus.Should().Be(ElementStatus.Approved);

            var parentElementResult = await Context.Elements.SingleAsync(e => e.Id == parentElement.Id);
            parentElementResult.EndDate.Should().Be(childElement.StartDate.PlusDays(-1));

            var cancelElementResult = await Context.Elements.SingleAsync(e => e.Id == cancelElement.Id);
            cancelElementResult.InternalStatus.Should().Be(ElementStatus.Cancelled);
            cancelElementResult.Comment.Should().Be(cancelReferralElement.PendingComment);

            var endElementResult = await Context.Elements.SingleAsync(e => e.Id == endElement.Id);
            endElementResult.EndDate.Should().Be(endReferralElement.PendingEndDate);

            var resultOldReferral = Context.Referrals.Single(r => r.Id == oldReferral.Id);
            resultOldReferral.Status.Should().Be(ReferralStatus.Ended);
        }

        [Test, Property("AsUser", "Approver"), Property("WithApprovalLimit", 1000)]
        public async Task CanRequestAmendment()
        {
            // Arrange
            var service = _fixture.BuildService()
                .Create();

            var provider = _fixture.BuildProvider()
                .Create();

            var oneOffElementType = _fixture.BuildElementType(service.Id)
                .With(et => et.CostType, ElementCostType.OneOff)
                .Create();

            var referral = _fixture.BuildReferral(ReferralStatus.AwaitingApproval)
                .With(r => r.AssignedBrokerEmail, ApiUser.Email)
                .Create();

            await Context.Referrals.AddAsync(referral);
            await Context.Services.AddAsync(service);
            await Context.Providers.AddAsync(provider);
            await Context.ElementTypes.AddAsync(oneOffElementType);
            await Context.Referrals.AddAsync(referral);
            await Context.SaveChangesAsync();

            Context.ChangeTracker.Clear();

            var request = _fixture.Create<AmendmentRequest>();

            var code = await Post($"/api/v1/referrals/{referral.Id}/care-package/request-amendment", request);

            code.Should().Be(HttpStatusCode.OK);

            var (carePackageCode, response) = await Get<CarePackageResponse>($"/api/v1/referrals/{referral.Id}/care-package");

            carePackageCode.Should().Be((int) HttpStatusCode.OK);
            var amendment = response.Amendments.Single();
            amendment.Comment.Should().Be(request.Comment);
            amendment.Status.Should().Be(AmendmentStatus.InProgress);
            amendment.RequestedAt.Should().Be(CurrentInstant);
        }

        [Test, Property("AsUser", "CareChargesOfficer"), Property("WithApprovalLimit", 1000)]
        public async Task CanRequestFollowUp()
        {
            // Arrange
            var service = _fixture.BuildService()
                .Create();

            var provider = _fixture.BuildProvider()
                .Create();

            var oneOffElementType = _fixture.BuildElementType(service.Id)
                .With(et => et.CostType, ElementCostType.OneOff)
                .Create();

            var referral = _fixture.BuildReferral(ReferralStatus.Approved)
                .Create();

            await Context.Referrals.AddAsync(referral);
            await Context.Services.AddAsync(service);
            await Context.Providers.AddAsync(provider);
            await Context.ElementTypes.AddAsync(oneOffElementType);
            await Context.Referrals.AddAsync(referral);
            await Context.SaveChangesAsync();

            Context.ChangeTracker.Clear();

            var request = _fixture.Create<FollowUpRequest>();

            var code = await Post($"/api/v1/referrals/{referral.Id}/care-package/request-follow-up", request);

            code.Should().Be(HttpStatusCode.OK);

            var (carePackageCode, response) = await Get<CarePackageResponse>($"/api/v1/referrals/{referral.Id}/care-package");

            carePackageCode.Should().Be((int) HttpStatusCode.OK);
            var followUp = response.FollowUps.Single();
            followUp.Comment.Should().Be(request.Comment);
            followUp.Date.Should().Be(request.Date);
            followUp.Status.Should().Be(FollowUpStatus.InProgress);
            followUp.RequestedAt.Should().Be(CurrentInstant);
            followUp.RequestedBy.Should().BeEquivalentTo(ApiUser.ToResponse());
        }
    }
}
