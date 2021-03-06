using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using BrokerageApi.Tests.V1.E2ETests;
using BrokerageApi.Tests.V1.Helpers;
using BrokerageApi.V1.Factories;
using BrokerageApi.V1.Infrastructure;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using X.PagedList;

namespace BrokerageApi.Tests.V1.Factories
{
    [TestFixture]
    public class ResponseFactoryTest
    {
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            AssertionOptions.EquivalencySteps.Insert<InstantComparer>();
            _fixture = FixtureHelpers.Fixture;
        }

        [Test]
        public void AuditResponseMapsCorrectly()
        {
            var referral = _fixture.BuildReferral().Create();

            var auditEvent = _fixture.BuildAuditEvent()
                .With(ae => ae.Metadata, "{ \"test\": \"test\" }")
                .With(ae => ae.Referral, referral)
                .Create();

            var response = auditEvent.ToResponse();

            response.Id.Should().Be(auditEvent.Id);
            response.Message.Should().Be(auditEvent.Message);
            response.CreatedAt.Should().Be(auditEvent.CreatedAt);
            response.EventType.Should().Be(auditEvent.EventType);
            response.SocialCareId.Should().Be(auditEvent.SocialCareId);
            response.UserId.Should().Be(auditEvent.UserId);
            response.Metadata.Should().BeEquivalentTo(JObject.Parse(auditEvent.Metadata));
            response.ReferralId.Should().Be(auditEvent.Referral.Id);
            response.FormName.Should().Be(auditEvent.Referral.FormName);
        }

        [Test]
        public void PageMetadataResponseMapsCorrectly()
        {
            var pageMetadata = _fixture.Create<IPagedList>();

            var response = pageMetadata.ToResponse();

            response.PageCount.Should().Be(pageMetadata.PageCount);
            response.TotalItemCount.Should().Be(pageMetadata.TotalItemCount);
            response.PageNumber.Should().Be(pageMetadata.PageNumber);
            response.PageSize.Should().Be(pageMetadata.PageSize);
            response.HasPreviousPage.Should().Be(pageMetadata.HasPreviousPage);
            response.HasNextPage.Should().Be(pageMetadata.HasNextPage);
            response.IsFirstPage.Should().Be(pageMetadata.IsFirstPage);
            response.IsLastPage.Should().Be(pageMetadata.IsLastPage);
            response.FirstItemOnPage.Should().Be(pageMetadata.FirstItemOnPage);
            response.LastItemOnPage.Should().Be(pageMetadata.LastItemOnPage);
        }

        [Test]
        public void ElementTypeResponseMapsCorrectly()
        {
            var service = _fixture.BuildService().Create();
            var elementType = _fixture.BuildElementType(service.Id).Create();

            var response = elementType.ToResponse();

            response.Id.Should().Be(elementType.Id);
            response.Name.Should().Be(elementType.Name);
            response.Type.Should().Be(elementType.Type);
            response.CostType.Should().Be(elementType.CostType);
            response.Billing.Should().Be(elementType.Billing);
            response.PaymentCycle.Should().Be(elementType.PaymentCycle);
            response.NonPersonalBudget.Should().Be(elementType.NonPersonalBudget);
            response.IsS117.Should().Be(elementType.IsS117);
            response.IsResidential.Should().Be(elementType.IsResidential);
            response.Service.Should().BeEquivalentTo(elementType.Service?.ToResponse());
        }

        [Test]
        public void ElementResponseMapsCorrectly()
        {
            var expectedReferralId = 1234;
            var grandParentElement = _fixture.BuildElement(1, 1)
                .With(e => e.InternalStatus, ElementStatus.InProgress)
                .Create();
            var parentElement = _fixture.BuildElement(1, 1)
                .With(e => e.InternalStatus, ElementStatus.InProgress)
                .With(e => e.ParentElement, grandParentElement)
                .With(e => e.ParentElementId, grandParentElement.Id)
                .Create();
            var suspensionElement = _fixture.BuildElement(1, 1)
                .With(e => e.InternalStatus, ElementStatus.Suspended)
                .With(e => e.IsSuspension, true)
                .Create();
            var element = _fixture.BuildElement(1, 1)
                .With(e => e.InternalStatus, ElementStatus.InProgress)
                .With(e => e.ParentElement, parentElement)
                .With(e => e.ParentElementId, parentElement.Id)
                .With(e => e.SuspensionElements, new List<Element> { suspensionElement })
                .Create();
            var expectedReferralElement = _fixture.BuildReferralElement(expectedReferralId, element.Id, true).Create();
            element.ReferralElements = new List<ReferralElement>
            {
                expectedReferralElement, _fixture.BuildReferralElement(expectedReferralId + 1, element.Id, true).Create()
            };
            var expectedSuspensionReferralElement = _fixture.BuildReferralElement(expectedReferralId, suspensionElement.Id, true).Create();
            suspensionElement.ReferralElements = new List<ReferralElement>
            {
                expectedSuspensionReferralElement, _fixture.BuildReferralElement(expectedReferralId + 1, suspensionElement.Id, true).Create()
            };

            var response = element.ToResponse(expectedReferralId);

            response.Id.Should().Be(element.Id);
            response.ElementType.Should().BeEquivalentTo(element.ElementType?.ToResponse());
            response.NonPersonalBudget.Should().Be(element.NonPersonalBudget);
            response.Provider.Should().BeEquivalentTo(element.Provider?.ToResponse());
            response.Details.Should().Be(element.Details);
            response.Status.Should().Be(element.Status);
            response.StartDate.Should().Be(element.StartDate);
            response.EndDate.Should().Be(element.EndDate);
            response.Monday.Should().Be(element.Monday);
            response.Tuesday.Should().Be(element.Tuesday);
            response.Wednesday.Should().Be(element.Wednesday);
            response.Thursday.Should().Be(element.Thursday);
            response.Friday.Should().Be(element.Friday);
            response.Saturday.Should().Be(element.Saturday);
            response.Sunday.Should().Be(element.Sunday);
            response.Quantity.Should().Be(element.Quantity);
            response.Cost.Should().Be(element.Cost);
            response.CreatedBy.Should().Be(element.CreatedBy);
            response.CreatedAt.Should().Be(element.CreatedAt);
            response.UpdatedAt.Should().Be(element.UpdatedAt);
            response.ParentElement.Should().BeEquivalentTo(parentElement.ToResponse(expectedReferralId, false));
            response.ParentElement.ParentElement.Should().BeNull();
            response.Comment.Should().Be(element.Comment);
            response.PendingEndDate.Should().Be(expectedReferralElement.PendingEndDate);
            response.PendingCancellation.Should().Be(expectedReferralElement.PendingCancellation);
            response.PendingComment.Should().Be(expectedReferralElement.PendingComment);
            response.IsSuspension.Should().Be(element.IsSuspension);

            var responseSuspensionElement = response.SuspensionElements.Single();
            responseSuspensionElement.Should().BeEquivalentTo(suspensionElement.ToResponse(expectedReferralId));
            responseSuspensionElement.PendingEndDate.Should().Be(expectedSuspensionReferralElement.PendingEndDate);
            responseSuspensionElement.PendingCancellation.Should().Be(expectedSuspensionReferralElement.PendingCancellation);
            responseSuspensionElement.PendingComment.Should().Be(expectedSuspensionReferralElement.PendingComment);
        }

        [Test]
        public void ReferralResponseMapsCorrectly([Values] ReferralStatus status)
        {
            var broker = _fixture.BuildUser().Create();
            var approver = _fixture.BuildUser().Create();
            var amendments = _fixture.BuildReferralAmendment().CreateMany();
            var followUps = _fixture.BuildReferralFollowUp().CreateMany();
            var referral = _fixture.BuildReferral(status)
                .With(r => r.AssignedBroker, broker)
                .With(r => r.AssignedApprover, approver)
                .With(r => r.ReferralAmendments, amendments.ToList)
                .With(c => c.ReferralFollowUps, followUps.ToList)
                .Create();

            var response = referral.ToResponse();

            response.Id.Should().Be(referral.Id);
            response.WorkflowId.Should().Be(referral.WorkflowId);
            response.WorkflowType.Should().Be(referral.WorkflowType);
            response.FormName.Should().Be(referral.FormName);
            response.SocialCareId.Should().Be(referral.SocialCareId);
            response.ResidentName.Should().Be(referral.ResidentName);
            response.PrimarySupportReason.Should().Be(referral.PrimarySupportReason);
            response.DirectPayments.Should().Be(referral.DirectPayments);
            response.UrgentSince.Should().Be(referral.UrgentSince);
            response.Status.Should().Be(referral.Status);
            response.Note.Should().Be(referral.Note);
            response.Comment.Should().Be(referral.Comment);
            response.StartedAt.Should().Be(referral.StartedAt);
            response.IsResidential.Should().Be(referral.IsResidential);
            response.CareChargeStatus.Should().Be(referral.CareChargeStatus);
            response.CareChargesConfirmedAt.Should().Be(referral.CareChargesConfirmedAt);
            response.CreatedAt.Should().Be(referral.CreatedAt);
            response.UpdatedAt.Should().Be(referral.UpdatedAt);
            response.AssignedBroker.Should().BeEquivalentTo(referral.AssignedBroker?.ToResponse());
            response.AssignedApprover.Should().BeEquivalentTo(referral.AssignedApprover?.ToResponse());
            response.AssignedTo.Should().Be(status == ReferralStatus.AwaitingApproval ? referral.AssignedApprover.Email : referral.AssignedBroker.Email);
            response.Amendments.Should().BeEquivalentTo(referral.ReferralAmendments.Select(a => a.ToResponse()));
            response.FollowUps.Should().BeEquivalentTo(referral.ReferralFollowUps.Select(f => f.ToResponse()));
        }

        [Test]
        public void CarePackageResponseMapsCorrectly([Values] ReferralStatus status)
        {
            var broker = _fixture.BuildUser().Create();
            var approver = _fixture.BuildUser().Create();
            var amendments = _fixture.BuildReferralAmendment().CreateMany();
            var followUps = _fixture.BuildReferralFollowUp().CreateMany();
            var carePackage = _fixture.BuildCarePackage()
                .With(c => c.Status, status)
                .With(c => c.AssignedBroker, broker)
                .With(c => c.AssignedApprover, approver)
                .With(c => c.ReferralAmendments, amendments.ToList)
                .With(c => c.ReferralFollowUps, followUps.ToList)
                .Create();

            var response = carePackage.ToResponse();

            response.Id.Should().Be(carePackage.Id);
            response.WorkflowId.Should().Be(carePackage.WorkflowId);
            response.WorkflowType.Should().Be(carePackage.WorkflowType);
            response.FormName.Should().Be(carePackage.FormName);
            response.SocialCareId.Should().Be(carePackage.SocialCareId);
            response.ResidentName.Should().Be(carePackage.ResidentName);
            response.PrimarySupportReason.Should().Be(carePackage.PrimarySupportReason);
            response.UrgentSince.Should().Be(carePackage.UrgentSince);
            response.CarePackageName.Should().Be(carePackage.CarePackageName);
            response.Status.Should().Be(carePackage.Status);
            response.Note.Should().Be(carePackage.Note);
            response.StartedAt.Should().Be(carePackage.StartedAt);
            response.IsResidential.Should().Be(carePackage.IsResidential);
            response.CareChargeStatus.Should().Be(carePackage.CareChargeStatus);
            response.CareChargesConfirmedAt.Should().Be(carePackage.CareChargesConfirmedAt);
            response.CreatedAt.Should().Be(carePackage.CreatedAt);
            response.UpdatedAt.Should().Be(carePackage.UpdatedAt);
            response.StartDate.Should().Be(carePackage.StartDate);
            response.WeeklyCost.Should().Be(carePackage.WeeklyCost);
            response.WeeklyPayment.Should().Be(carePackage.WeeklyPayment);
            response.OneOffPayment.Should().Be(carePackage.OneOffPayment);
            response.Elements.Should().BeEquivalentTo(carePackage.ReferralElements.Select(re => re.Element.ToResponse(re.ReferralId)).ToList());
            response.Comment.Should().Be(carePackage.Comment);
            response.AssignedBroker.Should().BeEquivalentTo(carePackage.AssignedBroker?.ToResponse());
            response.AssignedApprover.Should().BeEquivalentTo(carePackage.AssignedApprover?.ToResponse());
            response.AssignedTo.Should().Be(status == ReferralStatus.AwaitingApproval ? carePackage.AssignedApprover.Email : carePackage.AssignedBroker.Email);
            response.EstimatedYearlyCost.Should().Be(carePackage.EstimatedYearlyCost);
            response.Amendments.Should().BeEquivalentTo(carePackage.ReferralAmendments.Select(a => a.ToResponse()));
            response.FollowUps.Should().BeEquivalentTo(carePackage.ReferralFollowUps.Select(f => f.ToResponse()));
        }

        [Test]
        public void UserResponseMapsCorrectly()
        {
            var user = _fixture.BuildUser().Create();

            var response = user.ToResponse();

            response.Id.Should().Be(user.Id);
            response.Name.Should().Be(user.Name);
            response.Email.Should().Be(user.Email);
            response.Roles.Should().BeEquivalentTo(user.Roles);
            response.IsActive.Should().Be(user.IsActive);
            response.CreatedAt.Should().Be(user.CreatedAt);
            response.UpdatedAt.Should().Be(user.UpdatedAt);
            response.ApprovalLimit.Should().Be(user.ApprovalLimit);
        }

        [Test]
        public void AmendmentResponseMapsCorrectly()
        {
            var amendment = _fixture.BuildReferralAmendment().Create();

            var response = amendment.ToResponse();

            response.Comment.Should().Be(amendment.Comment);
            response.Status.Should().Be(amendment.Status);
            response.RequestedAt.Should().Be(amendment.RequestedAt);
        }

        [Test]
        public void ServiceOverviewResponseMapsCorrectly()
        {
            var serviceOverview = _fixture.BuildServiceOverview().Create();
            var response = serviceOverview.ToResponse();

            response.Id.Should().Be(serviceOverview.Id);
            response.Name.Should().Be(serviceOverview.Name);
            response.StartDate.Should().Be(serviceOverview.StartDate);
            response.EndDate.Should().Be(serviceOverview.EndDate);
            response.WeeklyCost.Should().Be(serviceOverview.WeeklyCost);
            response.WeeklyPayment.Should().Be(serviceOverview.WeeklyPayment);
            response.AnnualCost.Should().Be(serviceOverview.AnnualCost);
            response.Status.Should().Be(serviceOverview.Status);
            response.Elements.Should().BeNull();
        }

        [Test]
        public void ServiceOverviewResponseWithElementsMapsCorrectly()
        {
            var referral = _fixture.BuildReferral().Create();
            var provider = _fixture.BuildProvider().Create();
            var elements = _fixture.BuildServiceOverviewElement()
                .With(e => e.Referral, referral)
                .With(e => e.Provider, provider)
                .CreateMany().ToList();

            var serviceOverview = _fixture.BuildServiceOverview()
                .With(so => so.Elements, elements)
                .Create();

            var response = serviceOverview.ToResponse();

            response.Id.Should().Be(serviceOverview.Id);
            response.Name.Should().Be(serviceOverview.Name);
            response.StartDate.Should().Be(serviceOverview.StartDate);
            response.EndDate.Should().Be(serviceOverview.EndDate);
            response.WeeklyCost.Should().Be(serviceOverview.WeeklyCost);
            response.WeeklyPayment.Should().Be(serviceOverview.WeeklyPayment);
            response.AnnualCost.Should().Be(serviceOverview.AnnualCost);
            response.Status.Should().Be(serviceOverview.Status);
            response.Elements.Should().BeEquivalentTo(elements.Select(e => e.ToResponse()).ToList());
        }

        [Test]
        public void ServiceOverviewElementResponseMapsCorrectly()
        {
            var referral = _fixture.BuildReferral().Create();
            var provider = _fixture.BuildProvider().Create();
            var element = _fixture.BuildServiceOverviewElement()
                .With(e => e.Referral, referral)
                .With(e => e.Provider, provider)
                .Create();

            var response = element.ToResponse();

            response.Id.Should().Be(element.Id);
            response.Type.Should().Be(element.Type);
            response.Name.Should().Be(element.Name);
            response.Referral.Should().BeEquivalentTo(referral.ToResponse());
            response.Provider.Should().BeEquivalentTo(provider.ToResponse());
            response.StartDate.Should().Be(element.StartDate);
            response.EndDate.Should().Be(element.EndDate);
            response.Status.Should().Be(element.Status);
            response.PaymentCycle.Should().Be(element.PaymentCycle);
            response.Quantity.Should().Be(element.Quantity);
            response.Cost.Should().Be(element.Cost);
        }

        [Test]
        public void ServiceUserResponseMapsCorrectly()
        {
            var serviceUser = _fixture.BuildServiceUser().Create();

            var response = serviceUser.ToResponse();

            response.SocialCareId.Should().Be(serviceUser.SocialCareId);
            response.ServiceUserName.Should().Be(serviceUser.ServiceUserName);
            response.DateOfBirth.Should().Be(serviceUser.DateOfBirth);
            response.CedarNumber.Should().Be(serviceUser.CedarNumber);
            response.CreatedAt.Should().Be(serviceUser.CreatedAt);
            response.UpdatedAt.Should().Be(serviceUser.UpdatedAt);

        }

        [Test]
        public void WorkflowResponseMapsCorrectly()
        {
            var workflow = _fixture.Build<Workflow>()
                .Without(w => w.Referral)
                .Create();

            var response = workflow.ToResponse();

            response.Id.Should().Be(workflow.Id);
            response.WorkflowType.Should().Be(workflow.WorkflowType);
            response.FormName.Should().Be(workflow.FormName);
            response.Note.Should().Be(workflow.Note);
            response.PrimarySupportReason.Should().Be(workflow.PrimarySupportReason);
            response.DirectPayments.Should().Be(workflow.DirectPayments);
            response.UrgentSince.Should().Be(workflow.UrgentSince);
            response.CreatedAt.Should().Be(workflow.CreatedAt);
        }
    }
}
