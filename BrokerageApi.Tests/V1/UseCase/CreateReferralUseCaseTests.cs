using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using System.Threading.Tasks;
using BrokerageApi.Tests.V1.Helpers;
using BrokerageApi.V1.Boundary.Request;
using BrokerageApi.V1.Factories;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.UseCase;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace BrokerageApi.Tests.V1.UseCase
{
    public class CreateReferralUseCaseTests
    {
        private CreateReferralUseCase _classUnderTest;
        private Fixture _fixture;
        private Mock<IReferralGateway> _mockReferralGateway;

        [SetUp]
        public void Setup()
        {
            _fixture = FixtureHelpers.Fixture;
            _mockReferralGateway = new MockReferralGateway();
            _classUnderTest = new CreateReferralUseCase(_mockReferralGateway.Object);
        }

        [Test]
        public async Task CreatesReferralFromRequest()
        {
            // Arrange
            var request = _fixture.Create<CreateReferralRequest>();

            // Act
            var result = await _classUnderTest.ExecuteAsync(request);

            // Assert
            result.Should().BeEquivalentTo(request.ToDatabase());
            _mockReferralGateway.Verify(m => m.CreateAsync(It.IsAny<Referral>()));
        }

        [Test]
        public async Task LinksPreviouslyApprovedElements()
        {
            var existingElements = _fixture.BuildElement(1, 1)
                .With(e => e.InternalStatus, ElementStatus.Approved)
                .CreateMany();

            var existingReferral = _fixture.BuildReferral(ReferralStatus.Approved)
                .With(r => r.Elements, existingElements.ToList())
                .Create();

            _mockReferralGateway
                .Setup(m => m.GetBySocialCareIdWithElementsAsync(existingReferral.SocialCareId))
                .ReturnsAsync(new List<Referral> { existingReferral });

            var request = _fixture.Build<CreateReferralRequest>()
                .With(r => r.SocialCareId, existingReferral.SocialCareId)
                .Create();

            var result = await _classUnderTest.ExecuteAsync(request);

            result.Elements.Should().BeEquivalentTo(existingElements);
        }

        [Test]
        public async Task DoesNotLinksPreviouslyNotApprovedElements()
        {
            var existingElements = (from ElementStatus status in Enum.GetValues(typeof(ElementStatus))
                                    where status != ElementStatus.Approved
                                    select _fixture.BuildElement(1, 1)
                                        .With(e => e.InternalStatus, status)
                                        .Create()).ToList();

            var existingReferral = _fixture.BuildReferral(ReferralStatus.Approved)
                .With(r => r.Elements, existingElements.ToList())
                .Create();

            _mockReferralGateway
                .Setup(m => m.GetBySocialCareIdWithElementsAsync(existingReferral.SocialCareId))
                .ReturnsAsync(new List<Referral> { existingReferral });

            var request = _fixture.Build<CreateReferralRequest>()
                .With(r => r.SocialCareId, existingReferral.SocialCareId)
                .Create();

            var result = await _classUnderTest.ExecuteAsync(request);

            result.Elements.Should().BeNullOrEmpty();
        }

        [Test]
        public async Task ThrowsInvalidOperationWhenInProgressReferralExists()
        {
            var existingReferral = _fixture.BuildReferral(ReferralStatus.InProgress)
                .Create();

            _mockReferralGateway
                .Setup(m => m.GetBySocialCareIdWithElementsAsync(existingReferral.SocialCareId))
                .ReturnsAsync(new List<Referral> { existingReferral });

            var request = _fixture.Build<CreateReferralRequest>()
                .With(r => r.SocialCareId, existingReferral.SocialCareId)
                .Create();

            Func<Task<Referral>> act = () => _classUnderTest.ExecuteAsync(request);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Existing in progress referral exists, please archive before raising new referral");
        }
    }

    public class MockReferralGateway : Mock<IReferralGateway>
    {
        public Referral LastReferral { get; private set; }
        public MockReferralGateway()
        {
            Setup(x => x.CreateAsync(It.IsAny<Referral>()))
                .Returns<Referral>(referral =>
                {
                    LastReferral = referral;
                    return Task.FromResult(referral);
                });
        }
    }
}
