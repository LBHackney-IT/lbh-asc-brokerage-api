using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using BrokerageApi.Tests.V1.Helpers;
using BrokerageApi.V1.Boundary.Request;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.Services.Interfaces;
using BrokerageApi.V1.UseCase.CarePackageCareCharges;
using FluentAssertions;
using Moq;
using NodaTime;
using NUnit.Framework;

namespace BrokerageApi.Tests.V1.UseCase.CarePackageCareCharges
{
    public class CreateCareChargeUseCaseTests
    {
        private CreateCareChargeUseCase _classUnderTest;
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

            _classUnderTest = new CreateCareChargeUseCase(
                _mockReferralGateway.Object,
                _mockElementTypeGateway.Object,
                _mockProviderGateway.Object,
                _mockUserService.Object,
                _mockClock.Object,
                _mockDbSaver.Object
            );
        }

        [Test]
        public async Task CreatesCareChargeFromRequest()
        {
            // Arrange
            var currentInstant = SystemClock.Instance.GetCurrentInstant();
            var elementType = _fixture.BuildElementType(1, ElementTypeType.ConfirmedCareCharge).Create();
            var provider = _fixture.BuildProvider().Create();
            const string expectedUserEmail = "expected@email.com";

            var referral = _fixture.BuildReferral(ReferralStatus.Approved)
                .Without(x => x.Elements)
                .Create();

            var request = _fixture.Build<CreateCareChargeRequest>()
                .With(x => x.ElementTypeId, elementType.Id)
                .Without(x => x.ParentElementId)
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
                .Returns(expectedUserEmail);

            _mockClock
                .SetupGet(x => x.Now)
                .Returns(currentInstant);

            // Act
            var result = await _classUnderTest.ExecuteAsync(referral.Id, request);

            // Assert
            result.Should().BeOfType<Element>();
            result.ElementType.Should().Be(elementType);
            result.CreatedAt.Should().Be(currentInstant);
            result.CreatedBy.Should().Be(expectedUserEmail);

            _mockReferralGateway.Verify(m => m.GetByIdWithElementsAsync(referral.Id));
            _mockElementTypeGateway.Verify(m => m.GetByIdAsync(elementType.Id));
            _mockClock.VerifyGet(x => x.Now, Times.Once());
            _mockDbSaver.VerifyChangesSaved();
        }

        [Test]
        public void ThrowsArgumentNullExceptionWhenReferralDoesntExist()
        {
            // Arrange
            var request = _fixture.Create<CreateCareChargeRequest>();

            _mockReferralGateway
                .Setup(x => x.GetByIdAsync(123456))
                .ReturnsAsync(null as Referral);

            _mockUserService
                .SetupGet(x => x.Email)
                .Returns("a.broker@hackney.gov.uk");

            // Act
            var exception = Assert.ThrowsAsync<ArgumentNullException>(
                async () => await _classUnderTest.ExecuteAsync(123456, request));

            // Assert
            Assert.That(exception.Message, Is.EqualTo("Referral not found for: 123456 (Parameter 'referralId')"));
            _mockDbSaver.VerifyChangesNotSaved();
        }

        [Test]
        public void ThrowsInvalidOperationExceptionWhenReferralIsNotInApproved()
        {
            // Arrange
            var request = _fixture.Create<CreateCareChargeRequest>();

            var referral = _fixture.BuildReferral(ReferralStatus.InProgress)
                .Create();

            _mockReferralGateway
                .Setup(x => x.GetByIdWithElementsAsync(referral.Id))
                .ReturnsAsync(referral);

            _mockUserService
                .SetupGet(x => x.Email)
                .Returns("a.broker@hackney.gov.uk");

            // Act
            var exception = Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _classUnderTest.ExecuteAsync(referral.Id, request));

            // Assert
            Assert.That(exception.Message, Is.EqualTo("Referral is not in a valid state for adding care charges"));
            _mockDbSaver.VerifyChangesNotSaved();
        }

        [Test]
        public void ThrowsArgumentExceptionWhenElementTypeDoesNotExist()
        {
            // Arrange
            var request = _fixture.Build<CreateCareChargeRequest>()
                .With(x => x.ElementTypeId, 123456)
                .Create();

            var referral = _fixture.BuildReferral(ReferralStatus.Approved)
                .With(x => x.AssignedBrokerEmail, "a.broker@hackney.gov.uk")
                .Create();

            _mockReferralGateway
                .Setup(x => x.GetByIdWithElementsAsync(referral.Id))
                .ReturnsAsync(referral);

            _mockUserService
                .SetupGet(x => x.Email)
                .Returns("a.broker@hackney.gov.uk");

            _mockElementTypeGateway
                .Setup(x => x.GetByIdAsync(123456))
                .ReturnsAsync(null as ElementType);

            // Act
            var exception = Assert.ThrowsAsync<ArgumentException>(
                async () => await _classUnderTest.ExecuteAsync(referral.Id, request));

            // Assert
            Assert.That(exception.Message, Is.EqualTo("Element type not found for: 123456"));
            _mockDbSaver.VerifyChangesNotSaved();
        }

        [Test]
        public async Task UnlinksParentElement()
        {
            // Arrange
            var currentInstant = SystemClock.Instance.GetCurrentInstant();
            var elementType = _fixture.BuildElementType(1, ElementTypeType.ConfirmedCareCharge).Create();
            var parentElement = _fixture.BuildElement(elementType.Id).Create();

            var referral = _fixture.BuildReferral(ReferralStatus.Approved)
                .With(x => x.AssignedBrokerEmail, "a.broker@hackney.gov.uk")
                .With(x => x.Elements, new List<Element> { parentElement })
                .Create();

            var request = _fixture.Build<CreateCareChargeRequest>()
                .With(x => x.ElementTypeId, elementType.Id)
                .With(x => x.ParentElementId, parentElement.Id)
                .Create();

            _mockReferralGateway
                .Setup(m => m.GetByIdWithElementsAsync(referral.Id))
                .ReturnsAsync(referral);

            _mockElementTypeGateway
                .Setup(m => m.GetByIdAsync(elementType.Id))
                .ReturnsAsync(elementType);

            _mockUserService
                .SetupGet(x => x.Email)
                .Returns("a.broker@hackney.gov.uk");

            _mockClock
                .SetupGet(x => x.Now)
                .Returns(currentInstant);

            // Act
            var result = await _classUnderTest.ExecuteAsync(referral.Id, request);

            // Assert
            Assert.That(result, Is.InstanceOf(typeof(Element)));
            Assert.That(result.ElementType, Is.EqualTo(elementType));

            referral.Elements.Should().NotContain(e => e.Id == parentElement.Id);
            _mockDbSaver.VerifyChangesSaved();
        }
    }
}
