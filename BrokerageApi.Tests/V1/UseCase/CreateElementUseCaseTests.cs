using AutoFixture;
using System;
using System.Threading.Tasks;
using NodaTime;
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
        private Mock<IReferralGateway> _mockReferralGateway;
        private Mock<IElementTypeGateway> _mockElementTypeGateway;
        private Mock<IProviderGateway> _mockProviderGateway;
        private Mock<IUserService> _mockUserService;
        private Mock<IClockService> _mockClock;
        private Mock<IDbSaver> _mockDbSaver;

        [SetUp]
        public void Setup()
        {
            _fixture = FixtureHelpers.Fixture;
            _mockReferralGateway = new Mock<IReferralGateway>();
            _mockElementTypeGateway = new Mock<IElementTypeGateway>();
            _mockProviderGateway = new Mock<IProviderGateway>();
            _mockUserService = new Mock<IUserService>();
            _mockClock = new Mock<IClockService>();
            _mockDbSaver = new Mock<IDbSaver>();

            _classUnderTest = new CreateElementUseCase(
                _mockReferralGateway.Object,
                _mockElementTypeGateway.Object,
                _mockProviderGateway.Object,
                _mockUserService.Object,
                _mockClock.Object,
                _mockDbSaver.Object
            );
        }

        [Test]
        public async Task CreatesElementFromRequest()
        {
            // Arrange
            var currentInstant = SystemClock.Instance.GetCurrentInstant();
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

            _mockReferralGateway
                .Setup(m => m.GetByIdAsync(referral.Id))
                .ReturnsAsync(referral);

            _mockElementTypeGateway
                .Setup(m => m.GetByIdAsync(elementType.Id))
                .ReturnsAsync(elementType);

            _mockProviderGateway
                .Setup(m => m.GetByIdAsync(provider.Id))
                .ReturnsAsync(provider);

            _mockUserService
                .SetupGet(x => x.Name)
                .Returns("a.broker@hackney.gov.uk");

            _mockClock
                .SetupGet(x => x.Now)
                .Returns(currentInstant);

            // Act
            var result = await _classUnderTest.ExecuteAsync(referral.Id, request);

            // Assert
            Assert.That(result, Is.InstanceOf(typeof(Element)));
            Assert.That(result.ElementType, Is.EqualTo(elementType));
            Assert.That(result.Provider, Is.EqualTo(provider));

            _mockReferralGateway.Verify(m => m.GetByIdAsync(referral.Id));
            _mockElementTypeGateway.Verify(m => m.GetByIdAsync(elementType.Id));
            _mockProviderGateway.Verify(m => m.GetByIdAsync(provider.Id));
            _mockClock.VerifyGet(x => x.Now, Times.Once());
            _mockDbSaver.Verify(x => x.SaveChangesAsync(), Times.Once());
        }

        [Test]
        public void ThrowsArgumentNullExceptionWhenReferralDoesntExist()
        {
            // Arrange
            var request = _fixture.Create<CreateElementRequest>();

            _mockReferralGateway
                .Setup(x => x.GetByIdAsync(123456))
                .ReturnsAsync(null as Referral);

            _mockUserService
                .SetupGet(x => x.Name)
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

            _mockReferralGateway
                .Setup(x => x.GetByIdAsync(referral.Id))
                .ReturnsAsync(referral);

            _mockUserService
                .SetupGet(x => x.Name)
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

            _mockReferralGateway
                .Setup(x => x.GetByIdAsync(referral.Id))
                .ReturnsAsync(referral);

            _mockUserService
                .SetupGet(x => x.Name)
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

            _mockReferralGateway
                .Setup(x => x.GetByIdAsync(referral.Id))
                .ReturnsAsync(referral);

            _mockUserService
                .SetupGet(x => x.Name)
                .Returns("a.broker@hackney.gov.uk");

            _mockElementTypeGateway
                .Setup(x => x.GetByIdAsync(123456))
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

            _mockReferralGateway
                .Setup(x => x.GetByIdAsync(referral.Id))
                .ReturnsAsync(referral);

            _mockUserService
                .SetupGet(x => x.Name)
                .Returns("a.broker@hackney.gov.uk");

            _mockElementTypeGateway
                .Setup(x => x.GetByIdAsync(elementType.Id))
                .ReturnsAsync(elementType);

            _mockProviderGateway
                .Setup(x => x.GetByIdAsync(123456))
                .ReturnsAsync(null as Provider);

            // Act
            var exception = Assert.ThrowsAsync<ArgumentException>(
                async () => await _classUnderTest.ExecuteAsync(referral.Id, request));

            // Assert
            Assert.That(exception.Message, Is.EqualTo("Provider not found for: 123456"));
        }
    }
}
