using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using BrokerageApi.Tests.V1.Helpers;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.UseCase.CarePackages;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace BrokerageApi.Tests.V1.UseCase.CarePackages
{

    public class GetApproversUseCaseTests
    {
        private Mock<ICarePackageGateway> _mockCarePackageGateway;
        private GetBudgetApproversUseCase _classUnderTest;
        private Fixture _fixture;
        private Mock<IUserGateway> _mockUserGateway;

        [SetUp]
        public void Setup()
        {
            _fixture = FixtureHelpers.Fixture;
            _mockCarePackageGateway = new Mock<ICarePackageGateway>();
            _mockUserGateway = new Mock<IUserGateway>();

            _classUnderTest = new GetBudgetApproversUseCase(
                _mockCarePackageGateway.Object,
                _mockUserGateway.Object
            );
        }

        [Test]
        public async Task GetApprovers()
        {
            // Arrange
            var carePackage = _fixture.Build<CarePackage>()
                .With(c => c.Status, ReferralStatus.InProgress)
                .Create();
            var approvers = _fixture.CreateMany<User>();
            var expectedYearlyCost = (carePackage.WeeklyPayment * 52) +
                                     (carePackage.Elements.Where(e => e.ElementType.CostType == ElementCostType.OneOff).Sum(e => e.Cost));

            _mockCarePackageGateway
                .Setup(x => x.GetByIdAsync(carePackage.Id))
                .ReturnsAsync(carePackage);
            _mockUserGateway
                .Setup(x => x.GetBudgetApproversAsync(carePackage.EstimatedYearlyCost))
                .ReturnsAsync(approvers);

            // Act
            var (resultApprovers, estimatedYearlyCost) = await _classUnderTest.ExecuteAsync(carePackage.Id);

            // Assert
            resultApprovers.Should().BeEquivalentTo(approvers);
            estimatedYearlyCost.Should().Be(expectedYearlyCost);
        }

        [Test]
        public async Task ThrowsArgumentNullExceptionWhenCarePackageDoesntExist()
        {
            // Arrange
            const int unknownReferralId = 123456;
            _mockCarePackageGateway
                .Setup(x => x.GetByIdAsync(unknownReferralId))
                .ReturnsAsync(null as CarePackage);

            // Act
            Func<Task<(IEnumerable<User> approvers, decimal estimatedYearlyCost)>> act = () => _classUnderTest.ExecuteAsync(unknownReferralId);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithMessage($"Care package not found for: {unknownReferralId} (Parameter 'referralId')");
        }

        [Test]
        public async Task ThrowsInvalidOperationExceptionWhenCarePackageIsNotInProgress([Values] ReferralStatus status)
        {
            // Arrange
            var carePackage = _fixture.Build<CarePackage>()
                .With(c => c.Status, status)
                .Create();

            _mockCarePackageGateway
                .Setup(x => x.GetByIdAsync(carePackage.Id))
                .ReturnsAsync(carePackage);

            // Act
            Func<Task<(IEnumerable<User> approvers, decimal estimatedYearlyCost)>> act = () => _classUnderTest.ExecuteAsync(carePackage.Id);

            // Assert
            if (status != ReferralStatus.InProgress)
            {
                await act.Should().ThrowAsync<InvalidOperationException>()
                    .WithMessage("Care package not in correct state");
            }
        }

        [Test]
        public async Task YearlyCostCalculatedCorrectly()
        {
            // Arrange
            var carePackage = _fixture.BuildCarePackage()
                .With(c => c.Status, ReferralStatus.InProgress)
                .Create();

            var dailyElementType = _fixture.BuildElementType(1)
                .With(et => et.CostType, ElementCostType.Daily)
                .Create();

            var oneOffElementType = _fixture.BuildElementType(1)
                .With(et => et.CostType, ElementCostType.OneOff)
                .Create();

            var dailyElements = _fixture.BuildElement(1, dailyElementType.Id)
                .With(e => e.InternalStatus, ElementStatus.Approved)
                .With(e => e.ElementType, dailyElementType)
                .CreateMany();

            var oneOffElements = _fixture.BuildElement(1, oneOffElementType.Id)
                .With(e => e.InternalStatus, ElementStatus.Approved)
                .With(e => e.ElementType, oneOffElementType)
                .CreateMany();

            carePackage.Elements = new List<Element>();
            carePackage.Elements.AddRange(dailyElements);
            carePackage.Elements.AddRange(oneOffElements);

            var approvers = _fixture.CreateMany<User>();
            var expectedYearlyCost = carePackage.WeeklyPayment * 52 +
                                     oneOffElements.Sum(e => e.Cost);

            _mockCarePackageGateway
                .Setup(x => x.GetByIdAsync(carePackage.Id))
                .ReturnsAsync(carePackage);
            _mockUserGateway
                .Setup(x => x.GetBudgetApproversAsync(carePackage.EstimatedYearlyCost))
                .ReturnsAsync(approvers);

            // Act
            var (resultApprovers, estimatedYearlyCost) = await _classUnderTest.ExecuteAsync(carePackage.Id);

            // Assert
            resultApprovers.Should().BeEquivalentTo(approvers);
            estimatedYearlyCost.Should().Be(expectedYearlyCost);
        }
    }
}
