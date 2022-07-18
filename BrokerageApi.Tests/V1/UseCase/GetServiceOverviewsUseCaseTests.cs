using System;
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
        private Mock<IServiceUserGateway> _mockServiceUserGateway;
        private Mock<IServiceOverviewGateway> _mockServiceOverviewGateway;
        private GetServiceOverviewsUseCase _classUnderTest;
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _fixture = FixtureHelpers.Fixture;
            _mockServiceUserGateway = new Mock<IServiceUserGateway>();
            _mockServiceOverviewGateway = new Mock<IServiceOverviewGateway>();

            _classUnderTest = new GetServiceOverviewsUseCase(
                _mockServiceUserGateway.Object,
                _mockServiceOverviewGateway.Object);
        }

        [Test]
        public async Task CanGetServiceOverviews()
        {
            // Arrange
            const string socialCareId = "expectedId";

            var serviceUser = _fixture.BuildServiceUser().Create();
            var serviceOverviews = _fixture.BuildServiceOverview().CreateMany();

            _mockServiceUserGateway
                .Setup(x => x.GetBySocialCareIdAsync(socialCareId))
                .ReturnsAsync(serviceUser);

            _mockServiceOverviewGateway
                .Setup(x => x.GetBySocialCareIdAsync(socialCareId))
                .ReturnsAsync(serviceOverviews);

            // Act
            var results = await _classUnderTest.ExecuteAsync(socialCareId);

            // Assert
            results.Should().BeEquivalentTo(serviceOverviews);
        }

        [Test]
        public async Task ThrowsExceptionWhenServiceUserNotFound()
        {
            // Arrange
            const string socialCareId = "unknownId";

            _mockServiceUserGateway
                .Setup(x => x.GetBySocialCareIdAsync(socialCareId))
                .ReturnsAsync((ServiceUser) null);

            // Act
            var act = () => _classUnderTest.ExecuteAsync(socialCareId);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithMessage($"Service user not found for: {socialCareId} (Parameter 'socialCareId')");
        }
    }
}
