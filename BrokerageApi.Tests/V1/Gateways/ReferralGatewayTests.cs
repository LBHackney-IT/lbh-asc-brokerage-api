using System;
using System.Threading.Tasks;
using BrokerageApi.V1.Gateways;
using BrokerageApi.V1.Infrastructure;
using FluentAssertions;
using FluentAssertions.Extensions;
using NUnit.Framework;

namespace BrokerageApi.Tests.V1.Gateways
{

    [TestFixture]
    public class ReferralGatewayTests : DatabaseTests
    {
        private ReferralGateway _classUnderTest;

        [SetUp]
        public void Setup()
        {
            _classUnderTest = new ReferralGateway(BrokerageContext);
        }

        [Test]
        public async Task CreatesReferral()
        {
            // Arrange
            var workflowId = "88114daf-788b-48af-917b-996420afbf61";
            var referral = BuildReferral(workflowId);

            // Act
            var result = await _classUnderTest.CreateAsync(referral);

            // Assert
            Assert.That(result, Is.EqualTo(referral));
            Assert.That(result.CreatedAt, Is.EqualTo(CurrentInstant));
            Assert.That(result.UpdatedAt, Is.EqualTo(CurrentInstant));
        }

        [Test]
        public async Task DoesNotCreateDuplicateReferrals()
        {
            // Arrange
            var workflowId = "88114daf-788b-48af-917b-996420afbf61";
            var referral = await AddReferral(workflowId);
            var duplicateReferral = BuildReferral(workflowId);

            // Act & Assert
            Assert.ThrowsAsync<Microsoft.EntityFrameworkCore.DbUpdateException>(
                () => _classUnderTest.CreateAsync(duplicateReferral)
            );
        }

        [Test]
        public async Task GetsCurrentReferrals()
        {
            // Arrange
            var unassignedReferral = new Referral()
            {
                WorkflowId = "3a386bf5-036d-47eb-ba58-704f3333e4fd",
                WorkflowType = WorkflowType.Assessment,
                FormName = "Care act assessment",
                SocialCareId = "33556688",
                ResidentName = "A Service User",
                PrimarySupportReason = "Physical Support",
                Status = ReferralStatus.Unassigned
            };

            var inReviewReferral = new Referral()
            {
                WorkflowId = "b018672b-a169-4b35-afa7-b8a9344073c1",
                WorkflowType = WorkflowType.Assessment,
                FormName = "Care act assessment",
                SocialCareId = "33556688",
                ResidentName = "A Service User",
                PrimarySupportReason = "Physical Support",
                Status = ReferralStatus.InReview
            };

            var assignedReferral = new Referral()
            {
                WorkflowId = "755caa62-3602-4229-90da-e30199a0336d",
                WorkflowType = WorkflowType.Assessment,
                FormName = "Care act assessment",
                SocialCareId = "33556688",
                ResidentName = "A Service User",
                PrimarySupportReason = "Physical Support",
                AssignedTo = "a.broker@hackney.gov.uk",
                Status = ReferralStatus.Assigned
            };

            var onHoldReferral = new Referral()
            {
                WorkflowId = "ff245519-a28e-426c-ad13-4459216a2b2f",
                WorkflowType = WorkflowType.Assessment,
                FormName = "Care act assessment",
                SocialCareId = "33556688",
                ResidentName = "A Service User",
                PrimarySupportReason = "Physical Support",
                Status = ReferralStatus.OnHold
            };

            var archivedReferral = new Referral()
            {
                WorkflowId = "c265bf16-dbc4-4d6d-afdf-9f9fd4ec7d14",
                WorkflowType = WorkflowType.Assessment,
                FormName = "Care act assessment",
                SocialCareId = "33556688",
                ResidentName = "A Service User",
                PrimarySupportReason = "Physical Support",
                Status = ReferralStatus.Archived
            };

            var inProgressReferral = new Referral()
            {
                WorkflowId = "3e48adb1-0ca2-456c-845a-efcd4eca4554",
                WorkflowType = WorkflowType.Assessment,
                FormName = "Care act assessment",
                SocialCareId = "33556688",
                ResidentName = "A Service User",
                PrimarySupportReason = "Physical Support",
                AssignedTo = "a.broker@hackney.gov.uk",
                Status = ReferralStatus.InProgress,
                StartedAt = CurrentInstant
            };

            var awaitingApprovalReferral = new Referral()
            {
                WorkflowId = "9cab0511-094f-4d6b-ba81-7246ec0dc716",
                WorkflowType = WorkflowType.Assessment,
                FormName = "Care act assessment",
                SocialCareId = "33556688",
                ResidentName = "A Service User",
                PrimarySupportReason = "Physical Support",
                AssignedTo = "a.broker@hackney.gov.uk",
                Status = ReferralStatus.AwaitingApproval
            };

            var approvedReferral = new Referral()
            {
                WorkflowId = "174079ae-75b4-43b4-9d29-363e88e7dd40",
                WorkflowType = WorkflowType.Assessment,
                FormName = "Care act assessment",
                SocialCareId = "33556688",
                ResidentName = "A Service User",
                PrimarySupportReason = "Physical Support",
                AssignedTo = "a.broker@hackney.gov.uk",
                Status = ReferralStatus.Approved
            };

            await BrokerageContext.Referrals.AddAsync(unassignedReferral);
            await BrokerageContext.Referrals.AddAsync(inReviewReferral);
            await BrokerageContext.Referrals.AddAsync(assignedReferral);
            await BrokerageContext.Referrals.AddAsync(onHoldReferral);
            await BrokerageContext.Referrals.AddAsync(archivedReferral);
            await BrokerageContext.Referrals.AddAsync(inProgressReferral);
            await BrokerageContext.Referrals.AddAsync(awaitingApprovalReferral);
            await BrokerageContext.Referrals.AddAsync(approvedReferral);
            await BrokerageContext.SaveChangesAsync();

            // Act
            var result = await _classUnderTest.GetCurrentAsync();

            Assert.That(result, Contains.Item(unassignedReferral));
            Assert.That(result, Contains.Item(inReviewReferral));
            Assert.That(result, Contains.Item(assignedReferral));
            Assert.That(result, Contains.Item(onHoldReferral));
            Assert.That(result, Contains.Item(inProgressReferral));
            Assert.That(result, Contains.Item(awaitingApprovalReferral));

            Assert.That(result, Does.Not.Contain(archivedReferral));
            Assert.That(result, Does.Not.Contain(approvedReferral));
        }

        [Test]
        public async Task GetsFilteredCurrentReferrals()
        {
            // Arrange
            var unassignedReferral = new Referral()
            {
                WorkflowId = "3a386bf5-036d-47eb-ba58-704f3333e4fd",
                WorkflowType = WorkflowType.Assessment,
                FormName = "Care act assessment",
                SocialCareId = "33556688",
                ResidentName = "A Service User",
                PrimarySupportReason = "Physical Support",
                Status = ReferralStatus.Unassigned
            };

            var inReviewReferral = new Referral()
            {
                WorkflowId = "b018672b-a169-4b35-afa7-b8a9344073c1",
                WorkflowType = WorkflowType.Assessment,
                FormName = "Care act assessment",
                SocialCareId = "33556688",
                ResidentName = "A Service User",
                PrimarySupportReason = "Physical Support",
                Status = ReferralStatus.InReview
            };

            await BrokerageContext.Referrals.AddAsync(unassignedReferral);
            await BrokerageContext.Referrals.AddAsync(inReviewReferral);
            await BrokerageContext.SaveChangesAsync();

            // Act
            var result = await _classUnderTest.GetCurrentAsync(ReferralStatus.Unassigned);

            Assert.That(result, Contains.Item(unassignedReferral));
            Assert.That(result, Does.Not.Contain(inReviewReferral));
        }

        [Test]
        public async Task GetsAssignedReferrals()
        {
            // Arrange
            var unassignedReferral = new Referral()
            {
                WorkflowId = "3a386bf5-036d-47eb-ba58-704f3333e4fd",
                WorkflowType = WorkflowType.Assessment,
                FormName = "Care act assessment",
                SocialCareId = "33556688",
                ResidentName = "A Service User",
                PrimarySupportReason = "Physical Support",
                Status = ReferralStatus.Unassigned
            };

            var inReviewReferral = new Referral()
            {
                WorkflowId = "b018672b-a169-4b35-afa7-b8a9344073c1",
                WorkflowType = WorkflowType.Assessment,
                FormName = "Care act assessment",
                SocialCareId = "33556688",
                ResidentName = "A Service User",
                PrimarySupportReason = "Physical Support",
                Status = ReferralStatus.InReview
            };

            var assignedReferral = new Referral()
            {
                WorkflowId = "755caa62-3602-4229-90da-e30199a0336d",
                WorkflowType = WorkflowType.Assessment,
                FormName = "Care act assessment",
                SocialCareId = "33556688",
                ResidentName = "A Service User",
                PrimarySupportReason = "Physical Support",
                AssignedTo = "a.broker@hackney.gov.uk",
                Status = ReferralStatus.Assigned
            };

            var otherAssignedReferral = new Referral()
            {
                WorkflowId = "501d5410-a6ca-4766-8080-23a3c2da374b",
                WorkflowType = WorkflowType.Assessment,
                FormName = "Care act assessment",
                SocialCareId = "33556688",
                ResidentName = "A Service User",
                PrimarySupportReason = "Physical Support",
                AssignedTo = "other.broker@hackney.gov.uk",
                Status = ReferralStatus.Assigned
            };

            var onHoldReferral = new Referral()
            {
                WorkflowId = "ff245519-a28e-426c-ad13-4459216a2b2f",
                WorkflowType = WorkflowType.Assessment,
                FormName = "Care act assessment",
                SocialCareId = "33556688",
                ResidentName = "A Service User",
                PrimarySupportReason = "Physical Support",
                Status = ReferralStatus.OnHold
            };

            var archivedReferral = new Referral()
            {
                WorkflowId = "c265bf16-dbc4-4d6d-afdf-9f9fd4ec7d14",
                WorkflowType = WorkflowType.Assessment,
                FormName = "Care act assessment",
                SocialCareId = "33556688",
                ResidentName = "A Service User",
                PrimarySupportReason = "Physical Support",
                Status = ReferralStatus.Archived
            };

            var inProgressReferral = new Referral()
            {
                WorkflowId = "3e48adb1-0ca2-456c-845a-efcd4eca4554",
                WorkflowType = WorkflowType.Assessment,
                FormName = "Care act assessment",
                SocialCareId = "33556688",
                ResidentName = "A Service User",
                PrimarySupportReason = "Physical Support",
                AssignedTo = "a.broker@hackney.gov.uk",
                Status = ReferralStatus.InProgress,
                StartedAt = CurrentInstant
            };

            var awaitingApprovalReferral = new Referral()
            {
                WorkflowId = "9cab0511-094f-4d6b-ba81-7246ec0dc716",
                WorkflowType = WorkflowType.Assessment,
                FormName = "Care act assessment",
                SocialCareId = "33556688",
                ResidentName = "A Service User",
                PrimarySupportReason = "Physical Support",
                AssignedTo = "a.broker@hackney.gov.uk",
                Status = ReferralStatus.AwaitingApproval
            };

            var approvedReferral = new Referral()
            {
                WorkflowId = "174079ae-75b4-43b4-9d29-363e88e7dd40",
                WorkflowType = WorkflowType.Assessment,
                FormName = "Care act assessment",
                SocialCareId = "33556688",
                ResidentName = "A Service User",
                PrimarySupportReason = "Physical Support",
                AssignedTo = "a.broker@hackney.gov.uk",
                Status = ReferralStatus.Approved
            };

            await BrokerageContext.Referrals.AddAsync(unassignedReferral);
            await BrokerageContext.Referrals.AddAsync(inReviewReferral);
            await BrokerageContext.Referrals.AddAsync(assignedReferral);
            await BrokerageContext.Referrals.AddAsync(otherAssignedReferral);
            await BrokerageContext.Referrals.AddAsync(onHoldReferral);
            await BrokerageContext.Referrals.AddAsync(archivedReferral);
            await BrokerageContext.Referrals.AddAsync(inProgressReferral);
            await BrokerageContext.Referrals.AddAsync(awaitingApprovalReferral);
            await BrokerageContext.Referrals.AddAsync(approvedReferral);
            await BrokerageContext.SaveChangesAsync();

            // Act
            var result = await _classUnderTest.GetAssignedAsync("a.broker@hackney.gov.uk");

            Assert.That(result, Contains.Item(assignedReferral));
            Assert.That(result, Contains.Item(inProgressReferral));
            Assert.That(result, Contains.Item(awaitingApprovalReferral));

            Assert.That(result, Does.Not.Contain(unassignedReferral));
            Assert.That(result, Does.Not.Contain(inReviewReferral));
            Assert.That(result, Does.Not.Contain(onHoldReferral));
            Assert.That(result, Does.Not.Contain(otherAssignedReferral));
            Assert.That(result, Does.Not.Contain(archivedReferral));
            Assert.That(result, Does.Not.Contain(approvedReferral));
        }

        [Test]
        public async Task GetsFilteredAssignedReferrals()
        {
            // Arrange
            var assignedReferral = new Referral()
            {
                WorkflowId = "3a386bf5-036d-47eb-ba58-704f3333e4fd",
                WorkflowType = WorkflowType.Assessment,
                FormName = "Care act assessment",
                SocialCareId = "33556688",
                ResidentName = "A Service User",
                PrimarySupportReason = "Physical Support",
                AssignedTo = "a.broker@hackney.gov.uk",
                Status = ReferralStatus.Assigned
            };

            var inProgressReferral = new Referral()
            {
                WorkflowId = "b018672b-a169-4b35-afa7-b8a9344073c1",
                WorkflowType = WorkflowType.Assessment,
                FormName = "Care act assessment",
                SocialCareId = "33556688",
                ResidentName = "A Service User",
                PrimarySupportReason = "Physical Support",
                AssignedTo = "a.broker@hackney.gov.uk",
                Status = ReferralStatus.InProgress,
                StartedAt = CurrentInstant
            };

            await BrokerageContext.Referrals.AddAsync(assignedReferral);
            await BrokerageContext.Referrals.AddAsync(inProgressReferral);
            await BrokerageContext.SaveChangesAsync();

            // Act
            var result = await _classUnderTest.GetAssignedAsync("a.broker@hackney.gov.uk", ReferralStatus.InProgress);

            Assert.That(result, Contains.Item(inProgressReferral));
            Assert.That(result, Does.Not.Contain(assignedReferral));
        }

        [Test]
        public async Task GetsReferralByWorkflowId()
        {
            // Arrange
            var workflowId = "88114daf-788b-48af-917b-996420afbf61";
            var referral = await AddReferral(workflowId);

            // Act
            var result = await _classUnderTest.GetByWorkflowIdAsync(workflowId);

            // Assert
            result.Should().BeEquivalentTo(referral);
        }

        [Test]
        public async Task GetsReferralById()
        {
            // Arrange
            var workflowId = "88114daf-788b-48af-917b-996420afbf61";
            var referral = await AddReferral(workflowId);

            // Act
            var result = await _classUnderTest.GetByIdAsync(referral.Id);

            // Assert
            result.Should().BeEquivalentTo(referral);
        }

        private async Task<Referral> AddReferral(string workflowId)
        {
            var referral = BuildReferral(workflowId);

            await BrokerageContext.Referrals.AddAsync(referral);
            await BrokerageContext.SaveChangesAsync();

            return referral;
        }

        private static Referral BuildReferral(string workflowId)
        {
            return new Referral
            {
                WorkflowId = workflowId,
                WorkflowType = WorkflowType.Assessment,
                FormName = "Care act assessment",
                SocialCareId = "33556688",
                ResidentName = "A Service User",
                PrimarySupportReason = "Physical Support"
            };
        }
    }
}
