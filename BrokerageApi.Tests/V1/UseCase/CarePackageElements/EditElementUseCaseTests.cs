using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using BrokerageApi.Tests.V1.Helpers;
using BrokerageApi.V1.Boundary.Request;
using BrokerageApi.V1.Factories;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.Services.Interfaces;
using BrokerageApi.V1.UseCase.CarePackageElements;
using FluentAssertions;
using Moq;
using NodaTime;
using NUnit.Framework;

namespace BrokerageApi.Tests.V1.UseCase.CarePackageElements
{
    public class EditElementUseCaseTests
    {
        private EditElementUseCase _classUnderTest;
        private Fixture _fixture;
        private Mock<IReferralGateway> _mockReferralGateway;
        private Mock<IElementTypeGateway> _mockElementTypeGateway;
        private Mock<IProviderGateway> _mockProviderGateway;
        private Mock<IUserService> _mockUserService;
        private Mock<IClockService> _mockClock;
        private MockDbSaver _mockDbSaver;

        [SetUp]
        public void Setup()
        {
            _fixture = FixtureHelpers.Fixture;
            _mockReferralGateway = new Mock<IReferralGateway>();
            _mockElementTypeGateway = new Mock<IElementTypeGateway>();
            _mockProviderGateway = new Mock<IProviderGateway>();
            _mockUserService = new Mock<IUserService>();
            _mockClock = new Mock<IClockService>();
            _mockDbSaver = new MockDbSaver();

            _classUnderTest = new EditElementUseCase(
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
            var (provider, elementType, element, referral) = CreateReferralWithElement();

            var request = _fixture.Build<EditElementRequest>()
                .With(x => x.ElementTypeId, elementType.Id)
                .With(x => x.ProviderId, provider.Id)
                .Create();

            _mockReferralGateway
                .Setup(m => m.GetByIdWithElementsAsync(referral.Id))
                .ReturnsAsync(referral);

            _mockElementTypeGateway
                .Setup(m => m.GetByIdAsync(elementType.Id))
                .ReturnsAsync(elementType);

            _mockProviderGateway
                .Setup(m => m.GetByIdAsync(provider.Id))
                .ReturnsAsync(provider);

            _mockUserService
                .SetupGet(x => x.Email)
                .Returns(referral.AssignedTo);

            _mockClock
                .SetupGet(x => x.Now)
                .Returns(currentInstant);

            // Act
            var result = await _classUnderTest.ExecuteAsync(referral.Id, element.Id, request);

            // Assert
            result.Should().BeEquivalentTo(request.ToDatabase(element));
            result.UpdatedAt.Should().Be(currentInstant);
            referral.UpdatedAt.Should().Be(currentInstant);

            _mockReferralGateway.Verify(m => m.GetByIdWithElementsAsync(referral.Id));
            _mockElementTypeGateway.Verify(m => m.GetByIdAsync(elementType.Id));
            _mockProviderGateway.Verify(m => m.GetByIdAsync(provider.Id));
            _mockClock.VerifyGet(x => x.Now);
            _mockDbSaver.VerifyChangesSaved();
        }

        [Test]
        public async Task ThrowsArgumentNullExceptionWhenReferralDoesntExist()
        {
            // Arrange
            var request = _fixture
                .Create<EditElementRequest>();

            var referralId = 123456;
            _mockReferralGateway
                .Setup(x => x.GetByIdWithElementsAsync(referralId))
                .ReturnsAsync(null as Referral);

            _mockUserService
                .SetupGet(x => x.Email)
                .Returns("a.broker@hackney.gov.uk");

            // Act
            Func<Task<Element>> act = () => _classUnderTest.ExecuteAsync(referralId, 78910, request);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithMessage($"Referral not found for: {referralId} (Parameter 'referralId')");
            _mockDbSaver.VerifyChangesNotSaved();
        }

        [Test]
        public async Task ThrowsArgumentNullExceptionWhenElementDoesntExist()
        {
            // Arrange
            var currentInstant = SystemClock.Instance.GetCurrentInstant();
            var (provider, elementType, element, referral) = CreateReferralWithElement();

            var elementId = 123456;
            var request = _fixture.Build<EditElementRequest>()
                .With(x => x.ElementTypeId, elementType.Id)
                .With(x => x.ProviderId, provider.Id)
                .Create();

            _mockReferralGateway
                .Setup(m => m.GetByIdWithElementsAsync(referral.Id))
                .ReturnsAsync(referral);

            _mockUserService
                .SetupGet(x => x.Email)
                .Returns(referral.AssignedTo);

            // Act
            Func<Task<Element>> act = () => _classUnderTest.ExecuteAsync(referral.Id, elementId, request);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithMessage($"Element not found for: {elementId} (Parameter 'elementId')");
            _mockDbSaver.VerifyChangesNotSaved();
        }

        [Test]
        public async Task ThrowsInvalidOperationExceptionWhenReferralIsNotInProgress()
        {
            // Arrange
            var request = _fixture
                .Create<EditElementRequest>();

            var (provider, elementType, element, referral) = CreateReferralWithElement(ReferralStatus.Approved);

            _mockReferralGateway
                .Setup(x => x.GetByIdWithElementsAsync(referral.Id))
                .ReturnsAsync(referral);

            _mockUserService
                .SetupGet(x => x.Email)
                .Returns(referral.AssignedTo);

            // Act
            Func<Task<Element>> act = () => _classUnderTest.ExecuteAsync(referral.Id, element.Id, request);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Referral is not in a valid state for editing");
            _mockDbSaver.VerifyChangesNotSaved();
        }

        [Test]
        public async Task ThrowsUnauthorizedAccessExceptionWhenReferralIsAssignedToSomeoneElse()
        {
            // Arrange
            var userEmail = "someone@else.com";
            var request = _fixture.Create<EditElementRequest>();

            var (provider, elementType, element, referral) = CreateReferralWithElement();

            _mockReferralGateway
                .Setup(x => x.GetByIdWithElementsAsync(referral.Id))
                .ReturnsAsync(referral);

            _mockUserService
                .SetupGet(x => x.Email)
                .Returns(userEmail);

            // Act
            Func<Task<Element>> act = () => _classUnderTest.ExecuteAsync(referral.Id, element.Id, request);

            // Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage($"Referral is not assigned to {userEmail}");
            _mockDbSaver.VerifyChangesNotSaved();
        }

        [Test]
        public async Task ThrowsArgumentExceptionWhenElementTypeDoesNotExist()
        {
            // Arrange
            var elementTypeId = 123456;
            var request = _fixture.Build<EditElementRequest>()
                .With(x => x.ElementTypeId, elementTypeId)
                .Create();

            var (provider, elementType, element, referral) = CreateReferralWithElement();

            _mockReferralGateway
                .Setup(x => x.GetByIdWithElementsAsync(referral.Id))
                .ReturnsAsync(referral);

            _mockUserService
                .SetupGet(x => x.Email)
                .Returns(referral.AssignedTo);

            _mockElementTypeGateway
                .Setup(x => x.GetByIdAsync(elementTypeId))
                .ReturnsAsync(null as ElementType);

            // Act
            Func<Task<Element>> act = () => _classUnderTest.ExecuteAsync(referral.Id, element.Id, request);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage($"Element type not found for: {elementTypeId}");
            _mockDbSaver.VerifyChangesNotSaved();
        }

        [Test]
        public async Task ThrowsArgumentExceptionWhenProviderDoesNotExist()
        {
            // Arrange
            var (provider, elementType, element, referral) = CreateReferralWithElement();

            var providerId = 123456;
            var request = _fixture.Build<EditElementRequest>()
                .With(x => x.ElementTypeId, elementType.Id)
                .With(x => x.ProviderId, providerId)
                .Create();

            _mockReferralGateway
                .Setup(x => x.GetByIdWithElementsAsync(referral.Id))
                .ReturnsAsync(referral);

            _mockUserService
                .SetupGet(x => x.Email)
                .Returns(referral.AssignedTo);

            _mockElementTypeGateway
                .Setup(x => x.GetByIdAsync(elementType.Id))
                .ReturnsAsync(elementType);

            _mockProviderGateway
                .Setup(x => x.GetByIdAsync(providerId))
                .ReturnsAsync(null as Provider);

            // Act
            Func<Task<Element>> act = () => _classUnderTest.ExecuteAsync(referral.Id, element.Id, request);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage($"Provider not found for: {providerId}");
            _mockDbSaver.VerifyChangesNotSaved();
        }
        private (Provider provider, ElementType elementType, Element element, Referral referral) CreateReferralWithElement(ReferralStatus referralStatus = ReferralStatus.InProgress)
        {

            var elementType = _fixture
                .Create<ElementType>();

            var provider = _fixture
                .Create<Provider>();

            var element = _fixture.BuildElement(provider.Id, elementType.Id)
                .Create();

            var referral = _fixture.Build<Referral>()
                .With(x => x.Status, referralStatus)
                .With(x => x.Elements, new List<Element> { element })
                .Create();

            return (provider, elementType, element, referral);
        }
    }
}
