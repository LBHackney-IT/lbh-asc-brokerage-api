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
using BrokerageApi.V1.UseCase.CarePackageCareCharges;
using FluentAssertions;
using Moq;
using NodaTime;
using NUnit.Framework;

namespace BrokerageApi.Tests.V1.UseCase.CarePackageCareCharges
{
    public class EditCareChargeUseCaseTests
    {
        private EditCareChargeUseCase _classUnderTest;
        private Fixture _fixture;
        private Mock<IReferralGateway> _mockReferralGateway;
        private Mock<IElementTypeGateway> _mockElementTypeGateway;
        private Mock<IUserService> _mockUserService;
        private Mock<IClockService> _mockClock;
        private MockDbSaver _mockDbSaver;

        [SetUp]
        public void Setup()
        {
            _fixture = FixtureHelpers.Fixture;
            _mockReferralGateway = new Mock<IReferralGateway>();
            _mockElementTypeGateway = new Mock<IElementTypeGateway>();
            _mockUserService = new Mock<IUserService>();
            _mockClock = new Mock<IClockService>();
            _mockDbSaver = new MockDbSaver();

            _classUnderTest = new EditCareChargeUseCase(
                _mockReferralGateway.Object,
                _mockElementTypeGateway.Object,
                _mockUserService.Object,
                _mockClock.Object,
                _mockDbSaver.Object
            );
        }

        [Test]
        public async Task UpdatesCareChargeFromRequest()
        {
            // Arrange
            var currentInstant = SystemClock.Instance.GetCurrentInstant();
            var (elementType, element, referral) = CreateReferralWithElement();

            var request = _fixture.Build<EditCareChargeRequest>()
                .With(x => x.ElementTypeId, elementType.Id)
                .Create();

            _mockReferralGateway
                .Setup(m => m.GetByIdWithElementsAsync(referral.Id))
                .ReturnsAsync(referral);

            _mockElementTypeGateway
                .Setup(m => m.GetByIdAsync(elementType.Id))
                .ReturnsAsync(elementType);

            _mockUserService
                .SetupGet(x => x.Email)
                .Returns(referral.AssignedBrokerEmail);

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
            _mockClock.VerifyGet(x => x.Now);
            _mockDbSaver.VerifyChangesSaved();
        }

        [Test]
        public async Task ThrowsArgumentNullExceptionWhenReferralDoesntExist()
        {
            // Arrange
            var request = _fixture
                .Create<EditCareChargeRequest>();

            var referralId = 123456;

            _mockReferralGateway
                .Setup(x => x.GetByIdWithElementsAsync(referralId))
                .ReturnsAsync(null as Referral);

            _mockUserService
                .SetupGet(x => x.Email)
                .Returns("a.broker@hackney.gov.uk");

            // Act
            var act = () => _classUnderTest.ExecuteAsync(referralId, 78910, request);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithMessage($"Referral not found for: {referralId} (Parameter 'referralId')");
            _mockDbSaver.VerifyChangesNotSaved();
        }

        [Test]
        public async Task ThrowsArgumentNullExceptionWhenElementDoesntExist()
        {
            // Arrange
            var (elementType, _, referral) = CreateReferralWithElement();

            var elementId = 123456;
            var request = _fixture.Build<EditCareChargeRequest>()
                .With(x => x.ElementTypeId, elementType.Id)
                .Create();

            _mockReferralGateway
                .Setup(m => m.GetByIdWithElementsAsync(referral.Id))
                .ReturnsAsync(referral);

            _mockUserService
                .SetupGet(x => x.Email)
                .Returns(referral.AssignedBrokerEmail);

            // Act
            var act = () => _classUnderTest.ExecuteAsync(referral.Id, elementId, request);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithMessage($"Element not found for: {elementId} (Parameter 'elementId')");
            _mockDbSaver.VerifyChangesNotSaved();
        }

        [Test]
        public async Task ThrowsInvalidOperationExceptionWhenReferralIsNotApproved()
        {
            // Arrange
            var request = _fixture
                .Create<EditCareChargeRequest>();

            var (_, element, referral) = CreateReferralWithElement(ReferralStatus.InProgress);

            _mockReferralGateway
                .Setup(x => x.GetByIdWithElementsAsync(referral.Id))
                .ReturnsAsync(referral);

            // Act
            var act = () => _classUnderTest.ExecuteAsync(referral.Id, element.Id, request);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Referral is not in a valid state for editing care charges");

            _mockDbSaver.VerifyChangesNotSaved();
        }

        [Test]
        public async Task ThrowsArgumentExceptionWhenElementTypeDoesNotExist()
        {
            // Arrange
            var elementTypeId = 123456;
            var request = _fixture.Build<EditCareChargeRequest>()
                .With(x => x.ElementTypeId, elementTypeId)
                .Create();

            var (_, element, referral) = CreateReferralWithElement();

            _mockReferralGateway
                .Setup(x => x.GetByIdWithElementsAsync(referral.Id))
                .ReturnsAsync(referral);

            _mockElementTypeGateway
                .Setup(x => x.GetByIdAsync(elementTypeId))
                .ReturnsAsync(null as ElementType);

            // Act
            var act = () => _classUnderTest.ExecuteAsync(referral.Id, element.Id, request);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage($"Element type not found for: {elementTypeId}");
            _mockDbSaver.VerifyChangesNotSaved();
        }

        private (ElementType elementType, Element element, Referral referral) CreateReferralWithElement(ReferralStatus referralStatus = ReferralStatus.Approved)
        {

            var elementType = _fixture.BuildElementType(1, ElementTypeType.ConfirmedCareCharge)
                .Create();

            var element = _fixture.BuildElement(elementType.Id)
                .Create();

            var referral = _fixture.BuildReferral(referralStatus)
                .With(x => x.Elements, new List<Element> { element })
                .Create();

            return (elementType, element, referral);
        }
    }
}
