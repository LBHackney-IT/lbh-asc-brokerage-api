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
            result.Should().BeEquivalentTo(referral);
            result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, 2.Seconds());
            result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, 2.Seconds());
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
                SocialCareId = "33556688",
                Name = "A Service User"
            };
        }
    }
}
