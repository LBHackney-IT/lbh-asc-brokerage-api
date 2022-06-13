using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using BrokerageApi.Tests.V1.Helpers;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.Services.Interfaces;
using BrokerageApi.V1.UseCase;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace BrokerageApi.Tests.V1.UseCase
{
    public class GetBudgetApprovalsUseCaseTests
    {
        private Mock<ICarePackageGateway> _mockCarePackageGateway;
        private Mock<IUserService> _mockUserService;
        private GetBudgetApprovalsUseCase _classUnderTest;
        private Fixture _fixture;
        private Mock<IUserGateway> _mockUserGateway;

        [SetUp]
        public void Setup()
        {
            _fixture = FixtureHelpers.Fixture;
            _mockCarePackageGateway = new Mock<ICarePackageGateway>();
            _mockUserService = new Mock<IUserService>();
            _mockUserGateway = new Mock<IUserGateway>();

            _classUnderTest = new GetBudgetApprovalsUseCase(
                _mockCarePackageGateway.Object,
                _mockUserService.Object,
                _mockUserGateway.Object
            );
        }

        [Test]
        public async Task CanGetApprovals()
        {
            var user = _fixture.BuildUser()
                .Create();

            var carePackages = _fixture.BuildCarePackage()
                .CreateMany();

            _mockUserService
                .Setup(x => x.Email)
                .Returns(user.Email);

            _mockUserGateway
                .Setup(x => x.GetByEmailAsync(user.Email))
                .ReturnsAsync(user);

            _mockCarePackageGateway
                .Setup(x => x.GetByBudgetApprovalLimitAsync(user.ApprovalLimit.Value))
                .ReturnsAsync(carePackages);

            var result = await _classUnderTest.ExecuteAsync();

            result.Should().BeEquivalentTo(carePackages);
        }

        [Test]
        public async Task ThrowsUnauthorizedAccessWhenUserHasNoApprovalLimit()
        {
            var user = _fixture.BuildUser()
                .Without(u => u.ApprovalLimit)
                .Create();

            _mockUserService
                .Setup(x => x.Email)
                .Returns(user.Email);

            _mockUserGateway
                .Setup(x => x.GetByEmailAsync(user.Email))
                .ReturnsAsync(user);

            Func<Task<IEnumerable<CarePackage>>> act = () => _classUnderTest.ExecuteAsync();

            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("User has no approval limit set");
        }
    }
}
