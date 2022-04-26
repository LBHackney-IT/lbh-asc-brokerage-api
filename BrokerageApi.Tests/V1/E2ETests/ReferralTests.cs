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
    public class ReferralResponseComparer : IEqualityComparer<ReferralResponse>
    {
        public bool Equals(ReferralResponse r1, ReferralResponse r2)
        {
            return r1.WorkflowId == r2.WorkflowId;
        }

        public int GetHashCode(ReferralResponse r)
        {
            return r.WorkflowId.GetHashCode();
        }
    }

    public class ReferralTests : IntegrationTests<Startup>
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test, Property("AsUser", "Referrer")]
        public async Task CanCreateReferral()
        {
            // Arrange
            var request = new CreateReferralRequest()
            {
                WorkflowId = "88114daf-788b-48af-917b-996420afbf61",
                WorkflowType = WorkflowType.Assessment,
                FormName = "Care act assessment",
                SocialCareId = "33556688",
                ResidentName = "A Service User",
                PrimarySupportReason = "Physical Support",
                UrgentSince = null,
                Note = "Some notes"
            };

            // Act
            var (code, response) = await Post<ReferralResponse>($"/api/v1/referrals", request);

            // Assert
            Assert.That(code, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.WorkflowId, Is.EqualTo("88114daf-788b-48af-917b-996420afbf61"));
            Assert.That(response.WorkflowType, Is.EqualTo(WorkflowType.Assessment));
            Assert.That(response.SocialCareId, Is.EqualTo("33556688"));
            Assert.That(response.ResidentName, Is.EqualTo("A Service User"));
            Assert.That(response.PrimarySupportReason, Is.EqualTo("Physical Support"));
            Assert.That(response.UrgentSince, Is.Null);
            Assert.That(response.AssignedTo, Is.Null);
            Assert.That(response.Status, Is.EqualTo(ReferralStatus.Unassigned));
            Assert.That(response.Note, Is.EqualTo("Some notes"));
            Assert.That(response.CreatedAt, Is.EqualTo(DateTime.UtcNow).Within(2).Seconds);
            Assert.That(response.UpdatedAt, Is.EqualTo(DateTime.UtcNow).Within(2).Seconds);
        }

        [Test, Property("AsUser", "Referrer")]
        public async Task CanCreateUrgentReferral()
        {
            // Arrange
            var urgentSince = DateTime.UtcNow;
            var request = new CreateReferralRequest()
            {
                WorkflowId = "88114daf-788b-48af-917b-996420afbf61",
                WorkflowType = WorkflowType.Assessment,
                FormName = "Care act assessment",
                SocialCareId = "33556688",
                ResidentName = "A Service User",
                PrimarySupportReason = "Physical Support",
                UrgentSince = urgentSince,
                Note = "Some notes"
            };

            // Act
            var (code, response) = await Post<ReferralResponse>($"/api/v1/referrals", request);

            // Assert
            Assert.That(code, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.WorkflowId, Is.EqualTo("88114daf-788b-48af-917b-996420afbf61"));
            Assert.That(response.WorkflowType, Is.EqualTo(WorkflowType.Assessment));
            Assert.That(response.SocialCareId, Is.EqualTo("33556688"));
            Assert.That(response.ResidentName, Is.EqualTo("A Service User"));
            Assert.That(response.PrimarySupportReason, Is.EqualTo("Physical Support"));
            Assert.That(response.UrgentSince, Is.EqualTo(urgentSince));
            Assert.That(response.AssignedTo, Is.Null);
            Assert.That(response.Status, Is.EqualTo(ReferralStatus.Unassigned));
            Assert.That(response.Note, Is.EqualTo("Some notes"));
            Assert.That(response.CreatedAt, Is.EqualTo(DateTime.UtcNow).Within(2).Seconds);
            Assert.That(response.UpdatedAt, Is.EqualTo(DateTime.UtcNow).Within(2).Seconds);
        }

        [Test, Property("AsUser", "BrokerageAssistant")]
        public async Task CanGetCurrentReferrals()
        {
            // Arrange
            var comparer = new ReferralResponseComparer();

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
                StartedAt = DateTime.UtcNow
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

            await Context.Referrals.AddAsync(unassignedReferral);
            await Context.Referrals.AddAsync(inReviewReferral);
            await Context.Referrals.AddAsync(assignedReferral);
            await Context.Referrals.AddAsync(onHoldReferral);
            await Context.Referrals.AddAsync(archivedReferral);
            await Context.Referrals.AddAsync(inProgressReferral);
            await Context.Referrals.AddAsync(awaitingApprovalReferral);
            await Context.Referrals.AddAsync(approvedReferral);
            await Context.SaveChangesAsync();

            // Act
            var (code, response) = await Get<List<ReferralResponse>>($"/api/v1/referrals/current");

            // Assert
            Assert.That(code, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response, Contains.Item(unassignedReferral.ToResponse()).Using(comparer));
            Assert.That(response, Contains.Item(inReviewReferral.ToResponse()).Using(comparer));
            Assert.That(response, Contains.Item(assignedReferral.ToResponse()).Using(comparer));
            Assert.That(response, Contains.Item(onHoldReferral.ToResponse()).Using(comparer));
            Assert.That(response, Contains.Item(inProgressReferral.ToResponse()).Using(comparer));
            Assert.That(response, Contains.Item(awaitingApprovalReferral.ToResponse()).Using(comparer));
            Assert.That(response, Does.Not.Contain(archivedReferral.ToResponse()).Using(comparer));
            Assert.That(response, Does.Not.Contain(approvedReferral.ToResponse()).Using(comparer));
        }

        [Test, Property("AsUser", "BrokerageAssistant")]
        public async Task CanGetFilteredCurrentReferrals()
        {
            // Arrange
            var comparer = new ReferralResponseComparer();

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

            await Context.Referrals.AddAsync(unassignedReferral);
            await Context.Referrals.AddAsync(inReviewReferral);
            await Context.SaveChangesAsync();

            Context.ChangeTracker.Clear();

            // Act
            var (code, response) = await Get<List<ReferralResponse>>($"/api/v1/referrals/current?status=Unassigned");

            // Assert
            Assert.That(code, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response, Contains.Item(unassignedReferral.ToResponse()).Using(comparer));
            Assert.That(response, Does.Not.Contain(inReviewReferral.ToResponse()).Using(comparer));
        }

        [Test, Property("AsUser", "Broker")]
        public async Task CanGetAssignedReferrals()
        {
            // Arrange
            var comparer = new ReferralResponseComparer();

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
                AssignedTo = "api.user@hackney.gov.uk",
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
                AssignedTo = "other.user@hackney.gov.uk",
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
                AssignedTo = "api.user@hackney.gov.uk",
                Status = ReferralStatus.InProgress,
                StartedAt = DateTime.UtcNow
            };

            var awaitingApprovalReferral = new Referral()
            {
                WorkflowId = "9cab0511-094f-4d6b-ba81-7246ec0dc716",
                WorkflowType = WorkflowType.Assessment,
                FormName = "Care act assessment",
                SocialCareId = "33556688",
                ResidentName = "A Service User",
                PrimarySupportReason = "Physical Support",
                AssignedTo = "api.user@hackney.gov.uk",
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
                AssignedTo = "api.user@hackney.gov.uk",
                Status = ReferralStatus.Approved
            };

            await Context.Referrals.AddAsync(unassignedReferral);
            await Context.Referrals.AddAsync(inReviewReferral);
            await Context.Referrals.AddAsync(assignedReferral);
            await Context.Referrals.AddAsync(otherAssignedReferral);
            await Context.Referrals.AddAsync(onHoldReferral);
            await Context.Referrals.AddAsync(archivedReferral);
            await Context.Referrals.AddAsync(inProgressReferral);
            await Context.Referrals.AddAsync(awaitingApprovalReferral);
            await Context.Referrals.AddAsync(approvedReferral);
            await Context.SaveChangesAsync();

            Context.ChangeTracker.Clear();

            // Act
            var (code, response) = await Get<List<ReferralResponse>>($"/api/v1/referrals/assigned");

            // Assert
            Assert.That(code, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response, Contains.Item(assignedReferral.ToResponse()).Using(comparer));
            Assert.That(response, Contains.Item(inProgressReferral.ToResponse()).Using(comparer));
            Assert.That(response, Contains.Item(awaitingApprovalReferral.ToResponse()).Using(comparer));
            Assert.That(response, Does.Not.Contain(unassignedReferral.ToResponse()).Using(comparer));
            Assert.That(response, Does.Not.Contain(inReviewReferral.ToResponse()).Using(comparer));
            Assert.That(response, Does.Not.Contain(otherAssignedReferral.ToResponse()).Using(comparer));
            Assert.That(response, Does.Not.Contain(onHoldReferral.ToResponse()).Using(comparer));
            Assert.That(response, Does.Not.Contain(archivedReferral.ToResponse()).Using(comparer));
            Assert.That(response, Does.Not.Contain(approvedReferral.ToResponse()).Using(comparer));
        }

        [Test, Property("AsUser", "BrokerageAssistant")]
        public async Task CanGetFilteredAssignedReferrals()
        {
            // Arrange
            var comparer = new ReferralResponseComparer();

            var assignedReferral = new Referral()
            {
                WorkflowId = "3a386bf5-036d-47eb-ba58-704f3333e4fd",
                WorkflowType = WorkflowType.Assessment,
                FormName = "Care act assessment",
                SocialCareId = "33556688",
                ResidentName = "A Service User",
                PrimarySupportReason = "Physical Support",
                AssignedTo = "api.user@hackney.gov.uk",
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
                AssignedTo = "api.user@hackney.gov.uk",
                Status = ReferralStatus.InProgress,
                StartedAt = DateTime.UtcNow
            };

            await Context.Referrals.AddAsync(assignedReferral);
            await Context.Referrals.AddAsync(inProgressReferral);
            await Context.SaveChangesAsync();

            Context.ChangeTracker.Clear();

            // Act
            var (code, response) = await Get<List<ReferralResponse>>($"/api/v1/referrals/assigned?status=InProgress");

            // Assert
            Assert.That(code, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response, Contains.Item(inProgressReferral.ToResponse()).Using(comparer));
            Assert.That(response, Does.Not.Contain(assignedReferral.ToResponse()).Using(comparer));
        }

        [Test, Property("AsUser", "BrokerageAssistant")]
        public async Task CanGetReferralById()
        {
            // Arrange
            var comparer = new ReferralResponseComparer();

            var referral = new Referral()
            {
                WorkflowId = "3a386bf5-036d-47eb-ba58-704f3333e4fd",
                WorkflowType = WorkflowType.Assessment,
                FormName = "Care act assessment",
                SocialCareId = "33556688",
                ResidentName = "A Service User",
                PrimarySupportReason = "Physical Support",
                Status = ReferralStatus.Unassigned
            };

            await Context.Referrals.AddAsync(referral);
            await Context.SaveChangesAsync();

            Context.ChangeTracker.Clear();

            // Act
            var (code, response) = await Get<ReferralResponse>($"/api/v1/referrals/{referral.Id}");

            // Assert
            Assert.That(code, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response, Is.EqualTo(referral.ToResponse()).Using(comparer));
        }

        [Test, Property("AsUser", "BrokerageAssistant")]
        public async Task CanAssignReferral()
        {
            var request = new AssignBrokerRequest()
            {
                Broker = "a.broker@hackney.gov.uk"
            };

            var referral = new Referral()
            {
                WorkflowId = "3a386bf5-036d-47eb-ba58-704f3333e4fd",
                WorkflowType = WorkflowType.Assessment,
                FormName = "Care act assessment",
                SocialCareId = "33556688",
                ResidentName = "A Service User",
                PrimarySupportReason = "Physical Support",
                Status = ReferralStatus.Unassigned
            };

            var broker = new User()
            {
                Name = "A Broker",
                Email = "a.broker@hackney.gov.uk",
                IsActive = true,
                Roles = new List<UserRole>() {
                    UserRole.Broker
                }
            };

            await Context.Referrals.AddAsync(referral);
            await Context.Users.AddAsync(broker);
            await Context.SaveChangesAsync();

            Context.ChangeTracker.Clear();

            // Act
            var (code, response) = await Post<ReferralResponse>($"/api/v1/referrals/{referral.Id}/assign", request);

            // Assert
            Assert.That(code, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Status, Is.EqualTo(ReferralStatus.Assigned));
            Assert.That(response.AssignedTo, Is.EqualTo("a.broker@hackney.gov.uk"));
        }

        [Test, Property("AsUser", "BrokerageAssistant")]
        public async Task CanReassignReferral()
        {
            var request = new AssignBrokerRequest()
            {
                Broker = "a.broker@hackney.gov.uk"
            };

            var referral = new Referral()
            {
                WorkflowId = "3a386bf5-036d-47eb-ba58-704f3333e4fd",
                WorkflowType = WorkflowType.Assessment,
                FormName = "Care act assessment",
                SocialCareId = "33556688",
                ResidentName = "A Service User",
                PrimarySupportReason = "Physical Support",
                Status = ReferralStatus.InProgress,
                StartedAt = DateTime.UtcNow,
                AssignedTo = "other.broker@hackney.gov.uk"
            };

            var broker = new User()
            {
                Name = "A Broker",
                Email = "a.broker@hackney.gov.uk",
                IsActive = true,
                Roles = new List<UserRole>() {
                    UserRole.Broker
                }
            };

            await Context.Referrals.AddAsync(referral);
            await Context.Users.AddAsync(broker);
            await Context.SaveChangesAsync();

            Context.ChangeTracker.Clear();

            // Act
            var (code, response) = await Post<ReferralResponse>($"/api/v1/referrals/{referral.Id}/reassign", request);

            // Assert
            Assert.That(code, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Status, Is.EqualTo(ReferralStatus.InProgress));
            Assert.That(response.AssignedTo, Is.EqualTo("a.broker@hackney.gov.uk"));
        }
    }
}
