using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using BrokerageApi.Tests.V1.Helpers;
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
    public class DeleteCareChargesUseCaseTests
    {
        private DeleteCareChargeUseCase _classUnderTest;
        private Mock<IReferralGateway> _mockReferralGateway;
        private Fixture _fixture;
        private Mock<IUserService> _mockUserService;
        private MockDbSaver _mockDbSaver;
        private Mock<IClockService> _mockClock;
        private Instant _currentInstant;

        [SetUp]
        public void Setup()
        {
            _fixture = FixtureHelpers.Fixture;
            _mockReferralGateway = new Mock<IReferralGateway>();
            _mockUserService = new Mock<IUserService>();
            _mockDbSaver = new MockDbSaver();
            _mockClock = new Mock<IClockService>();
            _currentInstant = SystemClock.Instance.GetCurrentInstant();
            _mockClock.SetupGet(x => x.Now)
                .Returns(_currentInstant);

            _classUnderTest = new DeleteCareChargeUseCase(
                _mockReferralGateway.Object,
                _mockUserService.Object,
                _mockDbSaver.Object,
                _mockClock.Object
            );
        }

        [Test]
        public async Task DeletesCareCharge()
        {
            const int elementId = 123;

            var elements = CreateCareCharges(elementId + 1).ToList();
            elements.Add(CreateCareCharge(elementId));
            var referral = CreateReferral(ReferralStatus.Approved, elements);

            _mockReferralGateway
                .Setup(x => x.GetByIdWithElementsAsync(It.IsAny<int>()))
                .ReturnsAsync(referral);

            await _classUnderTest.ExecuteAsync(referral.Id, elementId);

            referral.Elements.Should().NotContain(e => e.Id == elementId);
            referral.UpdatedAt.Should().Be(_currentInstant);
            _mockDbSaver.VerifyChangesSaved();
        }

        [Test]
        public async Task ThrowsArgumentNullExceptionWhenReferralDoesntExist()
        {
            const int referralId = 123;

            _mockReferralGateway
                .Setup(x => x.GetByIdWithElementsAsync(It.IsAny<int>()))
                .ReturnsAsync(null as Referral);

            var act = () => _classUnderTest.ExecuteAsync(referralId, 456);

            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithMessage($"Referral not found for: {referralId} (Parameter 'referralId')");
        }

        [Test]
        public async Task ThrowsArgumentNullExceptionWhenElementDoesntExist()
        {
            const int elementId = 123;

            var elements = CreateCareCharges(elementId + 1);
            var referral = CreateReferral(ReferralStatus.Approved, elements.ToList());

            _mockReferralGateway
                .Setup(x => x.GetByIdWithElementsAsync(It.IsAny<int>()))
                .ReturnsAsync(referral);

            var act = () => _classUnderTest.ExecuteAsync(referral.Id, elementId);

            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithMessage($"Element not found for: {elementId} (Parameter 'elementId')");
        }

        [Test]
        public async Task ThrowsInvalidOperationExceptionWhenReferralNotApproved([Values] ReferralStatus status)
        {
            const int elementId = 123;

            var referral = CreateReferral(status);

            _mockReferralGateway
                .Setup(x => x.GetByIdWithElementsAsync(It.IsAny<int>()))
                .ReturnsAsync(referral);

            var act = () => _classUnderTest.ExecuteAsync(referral.Id, elementId);

            if (status != ReferralStatus.Approved)
            {
                await act.Should().ThrowAsync<InvalidOperationException>()
                    .WithMessage("Referral is not in a valid state for deleting care charges");
            }
            else
            {
                await act.Should().NotThrowAsync<InvalidOperationException>();
            }
        }

        [Test]
        public async Task DeleteCareChargeRelinksParent()
        {
            var parentElement = _fixture.BuildElement(1, 1)
                .Create();
            var childElement = _fixture.BuildElement(1, 1)
                .With(e => e.ParentElementId, parentElement.Id)
                .With(e => e.ParentElement, parentElement)
                .Create();
            var elements = new List<Element> { childElement };
            var referral = CreateReferral(ReferralStatus.Approved, elements);

            _mockReferralGateway
                .Setup(x => x.GetByIdWithElementsAsync(It.IsAny<int>()))
                .ReturnsAsync(referral);

            await _classUnderTest.ExecuteAsync(referral.Id, childElement.Id);

            referral.Elements.Should().NotContain(e => e.Id == childElement.Id);
            referral.Elements.Should().Contain(parentElement);
            referral.UpdatedAt.Should().Be(_currentInstant);
            _mockDbSaver.VerifyChangesSaved();
        }

        [Test]
        public async Task DeleteCareChargeRemovesSelfFromSuspended()
        {
            var suspendedElement = _fixture.BuildElement(1, 1)
                .Create();
            var element = _fixture.BuildElement(1, 1)
                .With(e => e.SuspendedElementId, suspendedElement.Id)
                .With(e => e.SuspendedElement, suspendedElement)
                .Create();
            suspendedElement.SuspensionElements = new List<Element>
            {
                element
            };

            var elements = new List<Element> { element };
            var referral = CreateReferral(ReferralStatus.Approved, elements);

            _mockReferralGateway
                .Setup(x => x.GetByIdWithElementsAsync(It.IsAny<int>()))
                .ReturnsAsync(referral);

            await _classUnderTest.ExecuteAsync(referral.Id, element.Id);

            suspendedElement.SuspensionElements.Should().NotContain(element);
            _mockDbSaver.VerifyChangesSaved();
        }

        private Referral CreateReferral(ReferralStatus referralStatus = ReferralStatus.Approved, List<Element> elements = null)
        {
            var referralBuilder = _fixture.BuildReferral(referralStatus)
                .With(r => r.Elements, elements)
                .With(r => r.UpdatedAt, _currentInstant.Minus(Duration.FromDays(1)));

            return referralBuilder.Create();
        }

        private IEnumerable<Element> CreateCareCharges(int minId = 0)
        {
            var elements = _fixture.BuildElement(1, 1)
                .With(e => e.Id, _fixture.CreateInt(minId, minId))
                .CreateMany();
            return elements;
        }

        private Element CreateCareCharge(int elementId)
        {
            var element = _fixture.BuildElement(1, 1)
                .With(e => e.Id, elementId)
                .Create();
            return element;
        }
    }
}
