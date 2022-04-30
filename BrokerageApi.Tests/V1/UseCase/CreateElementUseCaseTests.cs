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

        [Test]
        public void ThrowsArgumentNullExceptionWhenReferralDoesntExist()
        {
            // Arrange
            var request = _fixture.Create<CreateElementRequest>();

            _referralGatewayMock.Setup(x => x.GetByIdAsync(123456))
                .ReturnsAsync(null as Referral);

            _userServiceMock.SetupGet(x => x.Name)
                .Returns("a.broker@hackney.gov.uk");

            // Act
            var exception = Assert.ThrowsAsync<ArgumentNullException>(
                async () => await _classUnderTest.ExecuteAsync(123456, request));

            // Assert
            Assert.That(exception.Message, Is.EqualTo("Referral not found for: 123456 (Parameter 'referralId')"));
        }

        [Test]
        public void ThrowsInvalidOperationExceptionWhenReferralIsNotInProgress()
        {
            // Arrange
            var request = _fixture.Create<CreateElementRequest>();

            var referral = _fixture.Build<Referral>()
                .With(x => x.Status, ReferralStatus.Assigned)
                .With(x => x.AssignedTo, "a.broker@hackney.go.uk")
                .Create();

            _referralGatewayMock.Setup(x => x.GetByIdAsync(referral.Id))
                .ReturnsAsync(referral);

            _userServiceMock.SetupGet(x => x.Name)
                .Returns("a.broker@hackney.gov.uk");

            // Act
            var exception = Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _classUnderTest.ExecuteAsync(referral.Id, request));

            // Assert
            Assert.That(exception.Message, Is.EqualTo("Referral is not in a valid state for editing"));
        }

        [Test]
        public void ThrowsUnauthorizedAccessExceptionWhenReferralIsAssignedToSomeoneElse()
        {
            // Arrange
            var request = _fixture.Create<CreateElementRequest>();

            var referral = _fixture.Build<Referral>()
                .With(x => x.Status, ReferralStatus.InProgress)
                .With(x => x.AssignedTo, "other.broker@hackney.gov.uk")
                .Create();

            _referralGatewayMock.Setup(x => x.GetByIdAsync(referral.Id))
                .ReturnsAsync(referral);

            _userServiceMock.SetupGet(x => x.Name)
                .Returns("a.broker@hackney.gov.uk");

            // Act
            var exception = Assert.ThrowsAsync<UnauthorizedAccessException>(
                async () => await _classUnderTest.ExecuteAsync(referral.Id, request));

            // Assert
            Assert.That(exception.Message, Is.EqualTo("Referral is not assigned to a.broker@hackney.gov.uk"));
        }

        [Test]
        public void ThrowsArgumentExceptionWhenElementTypeDoesNotExist()
        {
            // Arrange
            var request = _fixture.Build<CreateElementRequest>()
                .With(x => x.ElementTypeId, 123456)
                .Create();

            var referral = _fixture.Build<Referral>()
                .With(x => x.Status, ReferralStatus.InProgress)
                .With(x => x.AssignedTo, "a.broker@hackney.gov.uk")
                .Create();

            _referralGatewayMock.Setup(x => x.GetByIdAsync(referral.Id))
                .ReturnsAsync(referral);

            _userServiceMock.SetupGet(x => x.Name)
                .Returns("a.broker@hackney.gov.uk");

            _elementTypeGatewayMock.Setup(x => x.GetByIdAsync(123456))
                .ReturnsAsync(null as ElementType);

            // Act
            var exception = Assert.ThrowsAsync<ArgumentException>(
                async () => await _classUnderTest.ExecuteAsync(referral.Id, request));

            // Assert
            Assert.That(exception.Message, Is.EqualTo("Element type not found for: 123456"));
        }

        [Test]
        public void ThrowsArgumentExceptionWhenProviderDoesNotExist()
        {
            // Arrange
            var elementType = _fixture.Create<ElementType>();

            var request = _fixture.Build<CreateElementRequest>()
                .With(x => x.ElementTypeId, elementType.Id)
                .With(x => x.ProviderId, 123456)
                .Create();

            var referral = _fixture.Build<Referral>()
                .With(x => x.Status, ReferralStatus.InProgress)
                .With(x => x.AssignedTo, "a.broker@hackney.gov.uk")
                .Create();

            _referralGatewayMock.Setup(x => x.GetByIdAsync(referral.Id))
                .ReturnsAsync(referral);

            _userServiceMock.SetupGet(x => x.Name)
                .Returns("a.broker@hackney.gov.uk");

            _elementTypeGatewayMock.Setup(x => x.GetByIdAsync(elementType.Id))
                .ReturnsAsync(elementType);

            _providerGatewayMock.Setup(x => x.GetByIdAsync(123456))
                .ReturnsAsync(null as Provider);

            // Act
            var exception = Assert.ThrowsAsync<ArgumentException>(
                async () => await _classUnderTest.ExecuteAsync(referral.Id, request));

            // Assert
            Assert.That(exception.Message, Is.EqualTo("Provider not found for: 123456"));
        }
    }
}
