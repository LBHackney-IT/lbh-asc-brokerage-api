using AutoFixture;
using System;
using System.Threading.Tasks;
using BrokerageApi.Tests.V1.Helpers;
using BrokerageApi.V1.Boundary.Request;
using BrokerageApi.V1.Factories;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.Services.Interfaces;
using BrokerageApi.V1.UseCase;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace BrokerageApi.Tests.V1.UseCase
{
    public class CreateElementUseCaseTests
    {
        private CreateElementUseCase _classUnderTest;
        private Fixture _fixture;
        private Mock<IReferralGateway> _referralGatewayMock;
        private Mock<IElementTypeGateway> _elementTypeGatewayMock;
        private Mock<IProviderGateway> _providerGatewayMock;
        private Mock<IUserService> _userServiceMock;
        private Mock<IDbSaver> _dbSaverMock;

        [SetUp]
        public void Setup()
        {
            _fixture = FixtureHelpers.Fixture;
            _referralGatewayMock = new Mock<IReferralGateway>();
            _elementTypeGatewayMock = new Mock<IElementTypeGateway>();
            _providerGatewayMock = new Mock<IProviderGateway>();
            _userServiceMock = new Mock<IUserService>();
            _dbSaverMock = new Mock<IDbSaver>();

            _classUnderTest = new CreateElementUseCase(
                _referralGatewayMock.Object,
                _elementTypeGatewayMock.Object,
                _providerGatewayMock.Object,
                _userServiceMock.Object,
                _dbSaverMock.Object
            );
        }

        [Test]
        public async Task CreatesElementFromRequest()
        {
            // Arrange
            var elementType = _fixture.Create<ElementType>();
            var provider = _fixture.Create<Provider>();

            var referral = _fixture.Build<Referral>()
                .With(x => x.Status, ReferralStatus.InProgress)
                .With(x => x.AssignedTo, "a.broker@hackney.gov.uk")
                .Create();

            var request = _fixture.Build<CreateElementRequest>()
                .With(x => x.ElementTypeId, elementType.Id)
                .With(x => x.ProviderId, provider.Id)
                .Create();

            _referralGatewayMock
                .Setup(m => m.GetByIdAsync(referral.Id))
                .ReturnsAsync(referral);

            _elementTypeGatewayMock
                .Setup(m => m.GetByIdAsync(elementType.Id))
                .ReturnsAsync(elementType);

            _providerGatewayMock
                .Setup(m => m.GetByIdAsync(provider.Id))
                .ReturnsAsync(provider);

            _userServiceMock
                .SetupGet(x => x.Name)
                .Returns("a.broker@hackney.gov.uk");

            // Act
            var result = await _classUnderTest.ExecuteAsync(referral.Id, request);

            // Assert
            Assert.That(result, Is.InstanceOf(typeof(Element)));
            Assert.That(result.ElementType, Is.EqualTo(elementType));
            Assert.That(result.Provider, Is.EqualTo(provider));

            _referralGatewayMock.Verify(m => m.GetByIdAsync(referral.Id));
            _elementTypeGatewayMock.Verify(m => m.GetByIdAsync(elementType.Id));
            _providerGatewayMock.Verify(m => m.GetByIdAsync(provider.Id));
            _dbSaverMock.Verify(x => x.SaveChangesAsync(), Times.Once());
        }
    }
}
