using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using BrokerageApi.Tests.V1.Helpers;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.UseCase;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace BrokerageApi.Tests.V1.UseCase
{
    public class GetServiceOverviewUseCaseTests
    {
        private Mock<IElementGateway> _mockElementGateway;
        private GetServiceOverviewUseCase _classUnderTest;
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _fixture = FixtureHelpers.Fixture;
            _mockElementGateway = new Mock<IElementGateway>();

            _classUnderTest = new GetServiceOverviewUseCase(_mockElementGateway.Object);
        }

        [Test]
        public async Task CanGetServiceOverview()
        {
            const string socialCareId = "expectedId";
            var elements = _fixture.BuildElement(1, 1)
                .With(e => e.SocialCareId, socialCareId)
                .CreateMany();
            _mockElementGateway.Setup(x => x.GetBySocialCareId(socialCareId))
                .ReturnsAsync(elements);

            var resultElements = await _classUnderTest.ExecuteAsync(socialCareId);

            resultElements.Should().BeEquivalentTo(elements);
        }

        [Test]
        public async Task ThrowsExceptionWhenNoElementsFound()
        {
            const string socialCareId = "expectedId";
            _mockElementGateway.Setup(x => x.GetBySocialCareId(socialCareId))
                .ReturnsAsync(new List<Element>());

            Func<Task<IEnumerable<Element>>> act = () => _classUnderTest.ExecuteAsync(socialCareId);

            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage($"Service overview not found for: {socialCareId}");
        }
    }
}
