using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using BrokerageApi.Tests.V1.Helpers;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.UseCase.CarePackageCareCharges;
using BrokerageApi.V1.UseCase.Interfaces.CarePackageCareCharges;
using FluentAssertions;
using Moq;
using NodaTime;
using NUnit.Framework;

namespace BrokerageApi.Tests.V1.UseCase.CarePackageCareCharges
{
    public class ResetCareChargeUseCaseTests
    {
        private Fixture _fixture;
        private ResetCareChargeUseCase _classUnderTest;
        private MockDbSaver _dbSaver;
        private Mock<IReferralGateway> _mockReferralGateway;
        private Mock<IDeleteCareChargeUseCase> _mockDeleteCareChargeUseCase;

        [SetUp]
        public void Setup()
        {
            _fixture = FixtureHelpers.Fixture;
            _mockReferralGateway = new Mock<IReferralGateway>();
            _mockDeleteCareChargeUseCase = new Mock<IDeleteCareChargeUseCase>();
            _dbSaver = new MockDbSaver();

            _classUnderTest = new ResetCareChargeUseCase(
                _mockReferralGateway.Object,
                _mockDeleteCareChargeUseCase.Object,
                _dbSaver.Object
            );
        }

        [Test]
        public async Task DeletesCareChargeWhenHasParentCareChargeAndInProgress()
        {
            var parentElement = _fixture.BuildElement(1, 1)
                .With(e => e.InternalStatus, ElementStatus.Approved)
                .Create();
            var element = _fixture.BuildElement(1, 1)
                .With(e => e.InternalStatus, ElementStatus.InProgress)
                .With(e => e.ParentElement, parentElement)
                .Create();
            var referral = CreateReferral(element);

            _mockReferralGateway
                .Setup(x => x.GetByIdWithElementsAsync(referral.Id))
                .ReturnsAsync(referral);

            await _classUnderTest.ExecuteAsync(referral.Id, element.Id);

            _mockDeleteCareChargeUseCase.Verify(x => x.ExecuteAsync(referral.Id, element.Id), Times.Once);
        }

        [Test]
        public async Task RemovesPendingValuesWhenCareChargeApproved()
        {
            var element = _fixture.BuildElement(1, 1)
                .With(e => e.InternalStatus, ElementStatus.Approved)
                .Create();
            var referral = CreateReferral(element);
            var referralElement = referral.ReferralElements.Single(re => re.ElementId == element.Id);

            _mockReferralGateway
                .Setup(x => x.GetByIdWithElementsAsync(referral.Id))
                .ReturnsAsync(referral);

            await _classUnderTest.ExecuteAsync(referral.Id, element.Id);

            referralElement.PendingEndDate.Should().BeNull();
            referralElement.PendingCancellation.Should().BeNull();
            referralElement.PendingComment.Should().BeNull();
            _dbSaver.VerifyChangesSaved();
            _mockDeleteCareChargeUseCase.Verify(x => x.ExecuteAsync(referral.Id, element.Id), Times.Never);
        }

        [Test]
        public async Task DeletesInProgressSuspensionCareChargesWhenApproved()
        {
            var inProgressSuspensionElements = _fixture.BuildElement(1, 1)
                .With(e => e.InternalStatus, ElementStatus.InProgress)
                .CreateMany();
            var approvedSuspensionElements = _fixture.BuildElement(1, 1)
                .With(e => e.InternalStatus, ElementStatus.Approved)
                .CreateMany();
            var allSuspensionElements = inProgressSuspensionElements.Concat(approvedSuspensionElements);
            var element = _fixture.BuildElement(1, 1)
                .With(e => e.InternalStatus, ElementStatus.Approved)
                .With(e => e.SuspensionElements, allSuspensionElements.ToList)
                .Create();

            var referral = CreateReferral(element);
            referral.ReferralElements.AddRange(allSuspensionElements.Select(e => _fixture.BuildReferralElement(referral.Id, e.Id).Create()));

            _mockReferralGateway
                .Setup(x => x.GetByIdWithElementsAsync(referral.Id))
                .ReturnsAsync(referral);

            await _classUnderTest.ExecuteAsync(referral.Id, element.Id);

            element.SuspensionElements.Should().BeEmpty();
            var elementIds = referral.ReferralElements.Select(re => re.ElementId);
            elementIds.Should().Contain(element.Id);
            elementIds.Should().Contain(approvedSuspensionElements.Select(e => e.Id));
            elementIds.Should().NotContain(inProgressSuspensionElements.Select(e => e.Id));
            _dbSaver.VerifyChangesSaved();
        }

        [Test]
        public async Task ThrowsInvalidOperationWhenCareChargeNotApprovedOrInProgress([Values] ElementStatus status)
        {
            var element = _fixture.BuildElement(1, 1)
                .With(e => e.InternalStatus, status)
                .Create();
            var referral = CreateReferral(element);

            _mockReferralGateway
                .Setup(x => x.GetByIdWithElementsAsync(referral.Id))
                .ReturnsAsync(referral);

            Func<Task> act = () => _classUnderTest.ExecuteAsync(referral.Id, element.Id);

            if (status != ElementStatus.Approved && status != ElementStatus.InProgress)
            {
                await act.Should().ThrowAsync<InvalidOperationException>()
                    .WithMessage($"Element {element.Id} is not in a valid state for reset");
                _dbSaver.VerifyChangesNotSaved();
            }
        }

        [Test]
        public async Task ThrowsArgumentNullExceptionWhenReferralNotFound()
        {
            const int unknownReferralId = 1234;
            const int unknownElementId = 1234;
            _mockReferralGateway.Setup(x => x.GetByIdAsync(unknownReferralId))
                .ReturnsAsync((Referral) null);

            Func<Task> act = () => _classUnderTest.ExecuteAsync(unknownReferralId, unknownElementId);

            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithMessage($"Referral not found {unknownReferralId} (Parameter 'referralId')");
            _dbSaver.VerifyChangesNotSaved();
        }

        [Test]
        public async Task ThrowsArgumentNullExceptionWhenElementNotFound()
        {
            const int unknownElementId = 1234;
            var referral = _fixture.BuildReferral().Create();
            _mockReferralGateway
                .Setup(x => x.GetByIdWithElementsAsync(referral.Id))
                .ReturnsAsync(referral);

            Func<Task> act = () => _classUnderTest.ExecuteAsync(referral.Id, unknownElementId);

            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithMessage($"Element not found {unknownElementId} (Parameter 'elementId')");
            _dbSaver.VerifyChangesNotSaved();
        }

        private Referral CreateReferral(params Element[] elements)
        {
            var referral = _fixture.BuildReferral()
                .With(r => r.Elements, elements.ToList())
                .Create();
            referral.ReferralElements = new List<ReferralElement>();

            foreach (var element in elements)
            {
                var referralElement = _fixture.BuildReferralElement(referral.Id, element.Id)
                    .With(re => re.PendingComment, "comment here")
                    .With(re => re.PendingEndDate, LocalDate.MaxIsoValue)
                    .With(re => re.PendingCancellation, true)
                    .Create();
                referral.ReferralElements.Add(referralElement);
            }
            return referral;
        }
    }
}
