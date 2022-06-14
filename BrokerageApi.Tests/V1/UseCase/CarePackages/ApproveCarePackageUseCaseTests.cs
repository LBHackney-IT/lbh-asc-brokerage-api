using System.Threading.Tasks;
using AutoFixture;
using BrokerageApi.Tests.V1.Helpers;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.Services.Interfaces;
using BrokerageApi.V1.UseCase.CarePackages;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace BrokerageApi.Tests.V1.UseCase.CarePackages
{
    public class ApproveCarePackageUseCaseTests
    {
        private Mock<ICarePackageGateway> _mockCarePackageGateway;
        private MockDbSaver _mockDbSaver;
        private Mock<IUserService> _mockUserService;
        private Mock<IUserGateway> _mockUserGateway;
        private Mock<IReferralGateway> _mockReferralGateway;
        private ApproveCarePackageUseCase _classUnderTest;
        private Fixture _fixture;
        private Mock<IAuditGateway> _mockAuditGateway;

        [SetUp]
        public void Setup()
        {
            _fixture = FixtureHelpers.Fixture;
            _mockCarePackageGateway = new Mock<ICarePackageGateway>();
            _mockReferralGateway = new Mock<IReferralGateway>();
            _mockUserService = new Mock<IUserService>();
            _mockUserGateway = new Mock<IUserGateway>();
            _mockDbSaver = new MockDbSaver();
            _mockAuditGateway = new Mock<IAuditGateway>();

            _classUnderTest = new ApproveCarePackageUseCase(
                _mockCarePackageGateway.Object,
                _mockReferralGateway.Object,
                _mockUserService.Object,
                _mockUserGateway.Object,
                _mockDbSaver.Object,
                _mockAuditGateway.Object
            );
        }

        [Test]
        public async Task UpdatesStatusToApproved()
        {
            var elements = _fixture.BuildElement(1, 1)
                .With(e => e.Status, ElementStatus.AwaitingApproval)
                .CreateMany();

            var referral = _fixture.BuildReferral(ReferralStatus.AwaitingApproval)
                .With(r => r.Elements, elements)
                .Create();

            _mockReferralGateway
                .Setup(x => x.GetByIdWithElementsAsync(referral.Id))
                .ReturnsAsync(referral);

            await _classUnderTest.ExecuteAsync(referral.Id);

            referral.Status.Should().Be(ReferralStatus.Approved);
            referral.Elements.Should().OnlyContain(e => e.Status == ElementStatus.Approved);

            _mockDbSaver.VerifyChangesSaved();
        }
    }
}
