using System;
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
    public class ReferralTests : IntegrationTests<Startup>
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public async Task CanCreateReferral()
        {
            // Arrange
            var request = new CreateReferralRequest()
            {
                WorkflowId = "88114daf-788b-48af-917b-996420afbf61",
                WorkflowType = WorkflowType.Assessment,
                SocialCareId = "33556688",
                Name = "A Service User"
            };

            // Act
            var (code, response) = await Post<ReferralResponse>($"/api/v1/referrals", request);

            // Assert
            Assert.That(code, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.WorkflowId, Is.EqualTo("88114daf-788b-48af-917b-996420afbf61"));
            Assert.That(response.WorkflowType, Is.EqualTo(WorkflowType.Assessment));
            Assert.That(response.SocialCareId, Is.EqualTo("33556688"));
            Assert.That(response.Name, Is.EqualTo("A Service User"));
            Assert.That(response.AssignedTo, Is.Null);
            Assert.That(response.Status, Is.EqualTo(ReferralStatus.Unassigned));
            Assert.That(response.CreatedAt, Is.EqualTo(DateTime.UtcNow).Within(2).Seconds);
            Assert.That(response.UpdatedAt, Is.EqualTo(DateTime.UtcNow).Within(2).Seconds);
        }
    }
}
