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
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            AssertionOptions.EquivalencySteps.Insert<InstantComparer>();
            _fixture = FixtureHelpers.Fixture;
        }

        [Test, Property("AsUser", "Referrer")]
        public async Task CanCreateReferral()
        {
            // Arrange
            var provider = _fixture.BuildProvider().Create();
            var service = _fixture.BuildService().Create();
            var elementType = _fixture.BuildElementType(service.Id).Create();
            var existingElements = _fixture.BuildElement(elementType.Id, provider.Id)
                .With(e => e.InternalStatus, ElementStatus.Approved)
                .CreateMany();
            var existingReferral = _fixture.BuildReferral(ReferralStatus.Approved)
                .With(r => r.Elements, existingElements.ToList)
                .Create();

            await Context.Providers.AddAsync(provider);
            await Context.Services.AddAsync(service);
            await Context.ElementTypes.AddAsync(elementType);
            await Context.Referrals.AddAsync(existingReferral);
            await Context.SaveChangesAsync();

            var request = _fixture.Build<CreateReferralRequest>()
                .With(r => r.SocialCareId, existingReferral.SocialCareId)
                .Create();

            // Act
            var (code, response) = await Post<ReferralResponse>($"/api/v1/referrals", request);

            // Assert
            code.Should().Be(HttpStatusCode.OK);
            response.WorkflowId.Should().Be(request.WorkflowId);
            response.WorkflowType.Should().Be(request.WorkflowType);
            response.SocialCareId.Should().Be(request.SocialCareId);
            response.ResidentName.Should().Be(request.ResidentName);
            response.PrimarySupportReason.Should().Be(request.PrimarySupportReason);
            response.DirectPayments.Should().Be(request.DirectPayments);
            response.UrgentSince.Should().Be(request.UrgentSince);
            response.Note.Should().Be(request.Note);

            response.AssignedBroker.Should().BeNull();
            response.Status.Should().Be(ReferralStatus.Unassigned);
            response.CreatedAt.Should().Be(CurrentInstant);
            response.UpdatedAt.Should().Be(CurrentInstant);

            var resultReferral = await Context.Referrals.SingleAsync(r => r.Id == response.Id);
            resultReferral.Elements.Should().BeEquivalentTo(existingElements);
        }

        [Test, Property("AsUser", "Referrer")]
        public async Task CanCreateUrgentReferral()
        {
            // Arrange
            var urgentSince = CurrentInstant;
            var request = new CreateReferralRequest()
            {
                WorkflowId = "88114daf-788b-48af-917b-996420afbf61",
                WorkflowType = WorkflowType.Assessment,
                FormName = "Care act assessment",
                SocialCareId = "33556688",
                ResidentName = "A Service User",
                PrimarySupportReason = "Physical Support",
                DirectPayments = "No",
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
            Assert.That(response.DirectPayments, Is.EqualTo("No"));
            Assert.That(response.UrgentSince, Is.EqualTo(urgentSince));
            Assert.That(response.AssignedBroker, Is.Null);
            Assert.That(response.Status, Is.EqualTo(ReferralStatus.Unassigned));
            Assert.That(response.Note, Is.EqualTo("Some notes"));
            Assert.That(response.CreatedAt, Is.EqualTo(CurrentInstant));
            Assert.That(response.UpdatedAt, Is.EqualTo(CurrentInstant));
        }

        [Test, Property("AsUser", "BrokerageAssistant")]
        public async Task CanGetCurrentReferrals()
        {
            // Arrange
            var brokerUser = _fixture.BuildUser().Create();
            var comparer = new ReferralResponseComparer();

            var unassignedReferral = new Referral()
            {
                WorkflowId = "3a386bf5-036d-47eb-ba58-704f3333e4fd",
                WorkflowType = WorkflowType.Assessment,
                FormName = "Care act assessment",
                SocialCareId = "33556681",
                ResidentName = "A Service User",
                PrimarySupportReason = "Physical Support",
                DirectPayments = "No",
                Status = ReferralStatus.Unassigned
            };

            var inReviewReferral = new Referral()
            {
                WorkflowId = "b018672b-a169-4b35-afa7-b8a9344073c1",
                WorkflowType = WorkflowType.Assessment,
                FormName = "Care act assessment",
                SocialCareId = "33556682",
                ResidentName = "A Service User",
                PrimarySupportReason = "Physical Support",
                DirectPayments = "No",
                Status = ReferralStatus.InReview
            };

            var assignedReferral = new Referral()
            {
                WorkflowId = "755caa62-3602-4229-90da-e30199a0336d",
                WorkflowType = WorkflowType.Assessment,
                FormName = "Care act assessment",
                SocialCareId = "33556683",
                ResidentName = "A Service User",
                PrimarySupportReason = "Physical Support",
                DirectPayments = "No",
                AssignedBrokerEmail = brokerUser.Email,
                Status = ReferralStatus.Assigned
            };

            var onHoldReferral = new Referral()
            {
                WorkflowId = "ff245519-a28e-426c-ad13-4459216a2b2f",
                WorkflowType = WorkflowType.Assessment,
                FormName = "Care act assessment",
                SocialCareId = "33556684",
                ResidentName = "A Service User",
                PrimarySupportReason = "Physical Support",
                DirectPayments = "No",
                Status = ReferralStatus.OnHold
            };

            var archivedReferral = new Referral()
            {
                WorkflowId = "c265bf16-dbc4-4d6d-afdf-9f9fd4ec7d14",
                WorkflowType = WorkflowType.Assessment,
                FormName = "Care act assessment",
                SocialCareId = "33556685",
                ResidentName = "A Service User",
                PrimarySupportReason = "Physical Support",
                DirectPayments = "No",
                Status = ReferralStatus.Archived
            };

            var inProgressReferral = new Referral()
            {
                WorkflowId = "3e48adb1-0ca2-456c-845a-efcd4eca4554",
                WorkflowType = WorkflowType.Assessment,
                FormName = "Care act assessment",
                SocialCareId = "33556686",
                ResidentName = "A Service User",
                PrimarySupportReason = "Physical Support",
                DirectPayments = "No",
                AssignedBrokerEmail = brokerUser.Email,
                Status = ReferralStatus.InProgress,
                StartedAt = CurrentInstant
            };

            var awaitingApprovalReferral = new Referral()
            {
                WorkflowId = "9cab0511-094f-4d6b-ba81-7246ec0dc716",
                WorkflowType = WorkflowType.Assessment,
                FormName = "Care act assessment",
                SocialCareId = "33556687",
                ResidentName = "A Service User",
                PrimarySupportReason = "Physical Support",
                DirectPayments = "No",
                AssignedBrokerEmail = brokerUser.Email,
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
                DirectPayments = "No",
                AssignedBrokerEmail = brokerUser.Email,
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
            await Context.Users.AddAsync(brokerUser);
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
                DirectPayments = "No",
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
                DirectPayments = "No",
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

        [Test, Property("AsUser", "CareChargesOfficer")]
        public async Task CanGetApprovedReferrals()
        {
            // Arrange
            var comparer = new ReferralResponseComparer();
            var otherUser = _fixture.BuildUser().Create();

            var unassignedReferral = new Referral()
            {
                WorkflowId = "3a386bf5-036d-47eb-ba58-704f3333e4fd",
                WorkflowType = WorkflowType.Assessment,
                FormName = "Care act assessment",
                SocialCareId = "33556681",
                ResidentName = "A Service User",
                PrimarySupportReason = "Physical Support",
                DirectPayments = "No",
                Status = ReferralStatus.Unassigned
            };

            var inReviewReferral = new Referral()
            {
                WorkflowId = "b018672b-a169-4b35-afa7-b8a9344073c1",
                WorkflowType = WorkflowType.Assessment,
                FormName = "Care act assessment",
                SocialCareId = "33556682",
                ResidentName = "A Service User",
                PrimarySupportReason = "Physical Support",
                DirectPayments = "No",
                Status = ReferralStatus.InReview
            };

            var assignedReferral = new Referral()
            {
                WorkflowId = "755caa62-3602-4229-90da-e30199a0336d",
                WorkflowType = WorkflowType.Assessment,
                FormName = "Care act assessment",
                SocialCareId = "33556683",
                ResidentName = "A Service User",
                PrimarySupportReason = "Physical Support",
                DirectPayments = "No",
                AssignedBrokerEmail = ApiUser.Email,
                Status = ReferralStatus.Assigned
            };

            var otherAssignedReferral = new Referral()
            {
                WorkflowId = "501d5410-a6ca-4766-8080-23a3c2da374b",
                WorkflowType = WorkflowType.Assessment,
                FormName = "Care act assessment",
                SocialCareId = "33556684",
                ResidentName = "A Service User",
                PrimarySupportReason = "Physical Support",
                DirectPayments = "No",
                AssignedBrokerEmail = otherUser.Email,
                Status = ReferralStatus.Assigned
            };

            var onHoldReferral = new Referral()
            {
                WorkflowId = "ff245519-a28e-426c-ad13-4459216a2b2f",
                WorkflowType = WorkflowType.Assessment,
                FormName = "Care act assessment",
                SocialCareId = "33556685",
                ResidentName = "A Service User",
                PrimarySupportReason = "Physical Support",
                DirectPayments = "No",
                Status = ReferralStatus.OnHold
            };

            var archivedReferral = new Referral()
            {
                WorkflowId = "c265bf16-dbc4-4d6d-afdf-9f9fd4ec7d14",
                WorkflowType = WorkflowType.Assessment,
                FormName = "Care act assessment",
                SocialCareId = "33556686",
                ResidentName = "A Service User",
                PrimarySupportReason = "Physical Support",
                Status = ReferralStatus.Archived
            };

            var inProgressReferral = new Referral()
            {
                WorkflowId = "3e48adb1-0ca2-456c-845a-efcd4eca4554",
                WorkflowType = WorkflowType.Assessment,
                FormName = "Care act assessment",
                SocialCareId = "33556687",
                ResidentName = "A Service User",
                PrimarySupportReason = "Physical Support",
                DirectPayments = "No",
                AssignedBrokerEmail = ApiUser.Email,
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
                DirectPayments = "No",
                AssignedBrokerEmail = ApiUser.Email,
                Status = ReferralStatus.AwaitingApproval
            };

            var approvedReferral = new Referral()
            {
                WorkflowId = "174079ae-75b4-43b4-9d29-363e88e7dd40",
                WorkflowType = WorkflowType.Assessment,
                FormName = "Care act assessment",
                SocialCareId = "33556689",
                ResidentName = "A Service User",
                PrimarySupportReason = "Physical Support",
                DirectPayments = "No",
                AssignedBrokerEmail = ApiUser.Email,
                Status = ReferralStatus.Approved
            };

            var approvedAndConfirmedReferral = new Referral()
            {
                WorkflowId = "82af1790-e591-013a-99f7-5a4fae5edecc",
                WorkflowType = WorkflowType.Assessment,
                FormName = "Care act assessment",
                SocialCareId = "33556690",
                ResidentName = "A Service User",
                PrimarySupportReason = "Physical Support",
                DirectPayments = "No",
                AssignedBrokerEmail = ApiUser.Email,
                Status = ReferralStatus.Approved,
                CareChargesConfirmedAt = CurrentInstant
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
            await Context.Referrals.AddAsync(approvedAndConfirmedReferral);
            await Context.Users.AddAsync(otherUser);
            await Context.SaveChangesAsync();

            Context.ChangeTracker.Clear();

            // Act
            var (code, response) = await Get<List<ReferralResponse>>($"/api/v1/referrals/approved");

            // Assert
            Assert.That(code, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response, Contains.Item(approvedReferral.ToResponse()).Using(comparer));
            Assert.That(response, Does.Not.Contain(assignedReferral.ToResponse()).Using(comparer));
            Assert.That(response, Does.Not.Contain(inProgressReferral.ToResponse()).Using(comparer));
            Assert.That(response, Does.Not.Contain(awaitingApprovalReferral.ToResponse()).Using(comparer));
            Assert.That(response, Does.Not.Contain(unassignedReferral.ToResponse()).Using(comparer));
            Assert.That(response, Does.Not.Contain(inReviewReferral.ToResponse()).Using(comparer));
            Assert.That(response, Does.Not.Contain(otherAssignedReferral.ToResponse()).Using(comparer));
            Assert.That(response, Does.Not.Contain(onHoldReferral.ToResponse()).Using(comparer));
            Assert.That(response, Does.Not.Contain(archivedReferral.ToResponse()).Using(comparer));
            Assert.That(response, Does.Not.Contain(approvedAndConfirmedReferral.ToResponse()).Using(comparer));
        }

        [Test, Property("AsUser", "Broker")]
        public async Task CanGetAssignedReferrals()
        {
            // Arrange
            var comparer = new ReferralResponseComparer();
            var otherUser = _fixture.BuildUser().Create();

            var unassignedReferral = new Referral()
            {
                WorkflowId = "3a386bf5-036d-47eb-ba58-704f3333e4fd",
                WorkflowType = WorkflowType.Assessment,
                FormName = "Care act assessment",
                SocialCareId = "33556681",
                ResidentName = "A Service User",
                PrimarySupportReason = "Physical Support",
                DirectPayments = "No",
                Status = ReferralStatus.Unassigned
            };

            var inReviewReferral = new Referral()
            {
                WorkflowId = "b018672b-a169-4b35-afa7-b8a9344073c1",
                WorkflowType = WorkflowType.Assessment,
                FormName = "Care act assessment",
                SocialCareId = "33556682",
                ResidentName = "A Service User",
                PrimarySupportReason = "Physical Support",
                DirectPayments = "No",
                Status = ReferralStatus.InReview
            };

            var assignedReferral = new Referral()
            {
                WorkflowId = "755caa62-3602-4229-90da-e30199a0336d",
                WorkflowType = WorkflowType.Assessment,
                FormName = "Care act assessment",
                SocialCareId = "33556683",
                ResidentName = "A Service User",
                PrimarySupportReason = "Physical Support",
                DirectPayments = "No",
                AssignedBrokerEmail = ApiUser.Email,
                Status = ReferralStatus.Assigned
            };

            var otherAssignedReferral = new Referral()
            {
                WorkflowId = "501d5410-a6ca-4766-8080-23a3c2da374b",
                WorkflowType = WorkflowType.Assessment,
                FormName = "Care act assessment",
                SocialCareId = "33556684",
                ResidentName = "A Service User",
                PrimarySupportReason = "Physical Support",
                DirectPayments = "No",
                AssignedBrokerEmail = otherUser.Email,
                Status = ReferralStatus.Assigned
            };

            var onHoldReferral = new Referral()
            {
                WorkflowId = "ff245519-a28e-426c-ad13-4459216a2b2f",
                WorkflowType = WorkflowType.Assessment,
                FormName = "Care act assessment",
                SocialCareId = "33556685",
                ResidentName = "A Service User",
                PrimarySupportReason = "Physical Support",
                DirectPayments = "No",
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
                SocialCareId = "33556686",
                ResidentName = "A Service User",
                PrimarySupportReason = "Physical Support",
                DirectPayments = "No",
                AssignedBrokerEmail = ApiUser.Email,
                Status = ReferralStatus.InProgress,
                StartedAt = CurrentInstant
            };

            var awaitingApprovalReferral = new Referral()
            {
                WorkflowId = "9cab0511-094f-4d6b-ba81-7246ec0dc716",
                WorkflowType = WorkflowType.Assessment,
                FormName = "Care act assessment",
                SocialCareId = "33556687",
                ResidentName = "A Service User",
                PrimarySupportReason = "Physical Support",
                DirectPayments = "No",
                AssignedBrokerEmail = ApiUser.Email,
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
                DirectPayments = "No",
                AssignedBrokerEmail = ApiUser.Email,
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
            await Context.Users.AddAsync(otherUser);
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
                DirectPayments = "No",
                AssignedBrokerEmail = "api.user@hackney.gov.uk",
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
                DirectPayments = "No",
                AssignedBrokerEmail = "api.user@hackney.gov.uk",
                Status = ReferralStatus.InProgress,
                StartedAt = CurrentInstant
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
            var user = _fixture.BuildUser().Create();

            var workflow = new Workflow()
            {
                Id = "3a386bf5-036d-47eb-ba58-704f3333e4fd",
                WorkflowType = WorkflowType.Assessment,
                FormName = "Care act assessment",
                PrimarySupportReason = "Physical Support",
                DirectPayments = "No"
            };

            var referral = new Referral()
            {
                WorkflowId = "3a386bf5-036d-47eb-ba58-704f3333e4fd",
                WorkflowType = WorkflowType.Assessment,
                Workflows = new List<Workflow> { workflow },
                FormName = "Care act assessment",
                SocialCareId = "33556688",
                ResidentName = "A Service User",
                PrimarySupportReason = "Physical Support",
                DirectPayments = "No",
                Comment = "A comment",
                Status = ReferralStatus.Unassigned,
                AssignedBrokerEmail = user.Email,
                ReferralAmendments = new List<ReferralAmendment>(),
                ReferralFollowUps = new List<ReferralFollowUp>(),
            };

            await Context.Referrals.AddAsync(referral);
            await Context.Users.AddAsync(user);
            await Context.SaveChangesAsync();

            Context.ChangeTracker.Clear();

            // Act
            var (code, response) = await Get<ReferralResponse>($"/api/v1/referrals/{referral.Id}");

            // Assert
            code.Should().Be(HttpStatusCode.OK);
            response.Should().BeEquivalentTo(referral.ToResponse());
            response.Comment.Should().BeEquivalentTo(referral.Comment);
            response.Workflows[0].Should().BeEquivalentTo(workflow.ToResponse());
            response.AssignedBroker.Should().BeEquivalentTo(user.ToResponse());
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
                DirectPayments = "No",
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
            Assert.That(response.AssignedBroker.Email, Is.EqualTo("a.broker@hackney.gov.uk"));
        }

        [Test, Property("AsUser", "BrokerageAssistant")]
        public async Task CanReassignReferral()
        {
            var broker = _fixture.BuildUser()
                .With(u => u.Roles, new List<UserRole>
                {
                    UserRole.Broker
                })
                .Create();
            var anotherBroker = _fixture.BuildUser()
                .With(u => u.Roles, new List<UserRole>
                {
                    UserRole.Broker
                })
                .Create();

            var request = new AssignBrokerRequest()
            {
                Broker = broker.Email
            };

            var referral = new Referral()
            {
                WorkflowId = "3a386bf5-036d-47eb-ba58-704f3333e4fd",
                WorkflowType = WorkflowType.Assessment,
                FormName = "Care act assessment",
                SocialCareId = "33556688",
                ResidentName = "A Service User",
                PrimarySupportReason = "Physical Support",
                DirectPayments = "No",
                Status = ReferralStatus.InProgress,
                StartedAt = CurrentInstant,
                AssignedBrokerEmail = anotherBroker.Email
            };

            await Context.Referrals.AddAsync(referral);
            await Context.Users.AddAsync(broker);
            await Context.Users.AddAsync(anotherBroker);
            await Context.SaveChangesAsync();

            Context.ChangeTracker.Clear();

            // Act
            var (code, response) = await Post<ReferralResponse>($"/api/v1/referrals/{referral.Id}/reassign", request);

            // Assert
            Assert.That(code, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Status, Is.EqualTo(ReferralStatus.InProgress));
            Assert.That(response.AssignedBroker.Email, Is.EqualTo(broker.Email));
        }

        [Test, Property("AsUser", "BrokerageAssistant")]
        public async Task AssignAddsAuditEvent()
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
                DirectPayments = "No",
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
            var auditEvent = await Context.AuditEvents.SingleOrDefaultAsync(ae => ae.EventType == AuditEventType.ReferralBrokerAssignment && ae.CreatedAt == Context.Clock.Now);
            auditEvent.Should().NotBeNull();
        }

        [Test, Property("AsUser", "Broker")]
        public async Task CanArchiveInProgressReferral()
        {
            var request = _fixture.Create<ArchiveReferralRequest>();

            var referral = _fixture.BuildReferral(ReferralStatus.InProgress)
                .Create();

            await Context.Referrals.AddAsync(referral);
            await Context.SaveChangesAsync();

            Context.ChangeTracker.Clear();

            // Act
            var code = await Post($"/api/v1/referrals/{referral.Id}/archive", request);

            // Assert
            code.Should().Be(HttpStatusCode.OK);
            var resultReferral = await Context.Referrals.SingleAsync(r => r.Id == referral.Id);
            resultReferral.Status.Should().Be(ReferralStatus.Archived);

            var auditEvent = await Context.AuditEvents.SingleOrDefaultAsync(ae => ae.EventType == AuditEventType.ReferralArchived && ae.CreatedAt == Context.Clock.Now);
            auditEvent.Should().NotBeNull();
        }

        [Test, Property("AsUser", "Broker")]
        public async Task CanArchiveAssignedReferral()
        {
            var request = _fixture.Create<ArchiveReferralRequest>();

            var referral = _fixture.BuildReferral(ReferralStatus.Assigned)
                .Create();

            await Context.Referrals.AddAsync(referral);
            await Context.SaveChangesAsync();

            Context.ChangeTracker.Clear();

            // Act
            var code = await Post($"/api/v1/referrals/{referral.Id}/archive", request);

            // Assert
            code.Should().Be(HttpStatusCode.OK);
            var resultReferral = await Context.Referrals.SingleAsync(r => r.Id == referral.Id);
            resultReferral.Status.Should().Be(ReferralStatus.Archived);

            var auditEvent = await Context.AuditEvents.SingleOrDefaultAsync(ae => ae.EventType == AuditEventType.ReferralArchived && ae.CreatedAt == Context.Clock.Now);
            auditEvent.Should().NotBeNull();
        }

        [Test, Property("AsUser", "Broker")]
        public async Task CanArchiveUnassignedReferral()
        {
            var request = _fixture.Create<ArchiveReferralRequest>();

            var referral = _fixture.BuildReferral(ReferralStatus.Unassigned)
                .Create();

            await Context.Referrals.AddAsync(referral);
            await Context.SaveChangesAsync();

            Context.ChangeTracker.Clear();

            // Act
            var code = await Post($"/api/v1/referrals/{referral.Id}/archive", request);

            // Assert
            code.Should().Be(HttpStatusCode.OK);
            var resultReferral = await Context.Referrals.SingleAsync(r => r.Id == referral.Id);
            resultReferral.Status.Should().Be(ReferralStatus.Archived);

            var auditEvent = await Context.AuditEvents.SingleOrDefaultAsync(ae => ae.EventType == AuditEventType.ReferralArchived && ae.CreatedAt == Context.Clock.Now);
            auditEvent.Should().NotBeNull();
        }


        [Test, Property("AsUser", "Approver"), Property("WithApprovalLimit", 1000)]
        public async Task CanGetApprovals()
        {
            const decimal approvalLimit = 1000;
            var provider = _fixture.BuildProvider()
                .Create();

            var service = _fixture.BuildService()
                .Create();

            var elementType = _fixture.BuildElementType(service.Id)
                .With(et => et.CostType, ElementCostType.OneOff)
                .With(et => et.CostOperation, MathOperation.Ignore)
                .With(et => et.PaymentOperation, MathOperation.Ignore)
                .Create();

            var belowLimitElement = _fixture.BuildElement(elementType.Id, provider.Id)
                .With(e => e.Cost, approvalLimit - 1)
                .Create();

            var aboveLimitElement = _fixture.BuildElement(elementType.Id, provider.Id)
                .With(e => e.Cost, approvalLimit + 1)
                .Create();

            var belowLimitReferrals = _fixture.BuildReferral(ReferralStatus.AwaitingApproval)
                .With(r => r.Elements, new List<Element>
                {
                    belowLimitElement
                })
                .CreateMany();

            var aboveLimitReferrals = _fixture.BuildReferral(ReferralStatus.AwaitingApproval)
                .With(r => r.Elements, new List<Element>
                {
                    aboveLimitElement
                })
                .CreateMany();

            await Context.Providers.AddAsync(provider);
            await Context.Services.AddAsync(service);
            await Context.ElementTypes.AddAsync(elementType);
            await Context.Referrals.AddRangeAsync(belowLimitReferrals);
            await Context.Referrals.AddRangeAsync(aboveLimitReferrals);
            await Context.SaveChangesAsync();

            Context.ChangeTracker.Clear();

            var (code, response) = await Get<List<CarePackageResponse>>($"/api/v1/referrals/budget-approvals");

            code.Should().Be((int) HttpStatusCode.OK);
            response.Select(r => r.Id).Should().Contain(belowLimitReferrals.Select(r => r.Id));
            response.Select(r => r.Id).Should().NotContain(aboveLimitReferrals.Select(r => r.Id));
        }
    }
}
