using System;
using System.Linq;
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
    public class GetServiceOverviewByIdUseCaseTests
    {
        private Mock<IServiceUserGateway> _mockServiceUserGateway;
        private Mock<IServiceOverviewGateway> _mockServiceOverviewGateway;
        private GetServiceOverviewByIdUseCase _classUnderTest;
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _fixture = FixtureHelpers.Fixture;
            _mockServiceUserGateway = new Mock<IServiceUserGateway>();
            _mockServiceOverviewGateway = new Mock<IServiceOverviewGateway>();

            _classUnderTest = new GetServiceOverviewByIdUseCase(
                _mockServiceUserGateway.Object,
                _mockServiceOverviewGateway.Object);
        }

        [Test]
        public async Task CanGetServiceOverviewById()
        {
            // Arrange
            const string socialCareId = "expectedId";
            const int serviceId = 1;

            var serviceUser = _fixture.BuildServiceUser().Create();
            var elements = _fixture.BuildServiceOverviewElement().CreateMany().ToList();
            var serviceOverview = _fixture.BuildServiceOverview().With(so => so.Elements, elements).Create();

            _mockServiceUserGateway
                .Setup(x => x.GetBySocialCareIdAsync(socialCareId))
                .ReturnsAsync(serviceUser);

            _mockServiceOverviewGateway
                .Setup(x => x.GetBySocialCareIdAndServiceIdAsync(socialCareId, serviceId))
                .ReturnsAsync(serviceOverview);

            // Act
            var result = await _classUnderTest.ExecuteAsync(socialCareId, serviceId);

            // Assert
            result.Should().BeEquivalentTo(serviceOverview);
        }

        [Test]
        public async Task ThrowsExceptionWhenServiceUserNotFound()
        {
            // Arrange
            const string socialCareId = "unknownId";
            const int serviceId = 1;

            _mockServiceUserGateway
                .Setup(x => x.GetBySocialCareIdAsync(socialCareId))
                .ReturnsAsync((ServiceUser) null);

            // Act
            var act = () => _classUnderTest.ExecuteAsync(socialCareId, serviceId);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithMessage($"Service user not found for: {socialCareId} (Parameter 'socialCareId')");
        }

        [Test]
        public async Task ThrowsExceptionWhenServiceNotFound()
        {
            // Arrange
            const string socialCareId = "expectedId";
            const int serviceId = 1;

            var serviceUser = _fixture.BuildServiceUser().Create();

            _mockServiceUserGateway
                .Setup(x => x.GetBySocialCareIdAsync(socialCareId))
                .ReturnsAsync(serviceUser);

            _mockServiceOverviewGateway
                .Setup(x => x.GetBySocialCareIdAndServiceIdAsync(socialCareId, serviceId))
                .ReturnsAsync((ServiceOverview) null);

            // Act
            var act = () => _classUnderTest.ExecuteAsync(socialCareId, serviceId);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithMessage($"Service not found for: {serviceId} (Parameter 'serviceId')");
        }
    }
}
