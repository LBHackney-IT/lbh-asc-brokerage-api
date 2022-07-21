using System;
using System.Linq;
using AutoFixture;
using System.Threading.Tasks;
using BrokerageApi.Tests.V1.Helpers;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.UseCase.ServiceUsers;
using BrokerageApi.V1.Factories;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace BrokerageApi.Tests.V1.UseCase.ServiceUsers
{
    public class EditServiceUserUseCaseTest
    {
        private Mock<IServiceUserGateway> _mockServiceUserGateway;
        private EditServiceUserUseCase _classUnderTest;
        private Fixture _fixture;
        private MockDbSaver _mockDbSaver;
        [SetUp]
        public void Setup()
        {
            _fixture = FixtureHelpers.Fixture;
            _mockServiceUserGateway = new Mock<IServiceUserGateway>();
            _mockDbSaver = new MockDbSaver();
            _classUnderTest = new EditServiceUserUseCase(_mockServiceUserGateway.Object, _mockDbSaver.Object);
        }
        [Test]
        public async Task EditsServiceUserCedarNumber()
        {
            //Arrange
            var serviceUser = _fixture.BuildServiceUser().Create();
            var serviceUserRequest = _fixture.BuildServiceUserRequest(serviceUser.SocialCareId)
            .Without(sur => sur.DateOfBirth)
            .Without(sur => sur.ServiceUserName)
            .Create();

            var request = _fixture.BuildEditServiceUserRequest(serviceUser.SocialCareId).Create();

            _mockServiceUserGateway
                .Setup(x => x.GetBySocialCareIdAsync(serviceUser.SocialCareId))
                .ReturnsAsync(serviceUser);

            //Act
            var result = await _classUnderTest.ExecuteAsync(request);

            //Assert
            result.Should().BeEquivalentTo(request.ToDatabase(serviceUser));
        }

        [Test]
        public async Task ThrowsArgumentNullExceptionWhenServiceUserDoesntExist()
        {

            var serviceUser = _fixture.BuildServiceUser().Create();
            var serviceUserRequest = _fixture.BuildServiceUserRequest(serviceUser.SocialCareId)
            .Without(sur => sur.DateOfBirth)
            .Without(sur => sur.ServiceUserName)
            .Create();

            var request = _fixture.BuildEditServiceUserRequest("fakeUserId").Create();

            _mockServiceUserGateway
                .Setup(x => x.GetBySocialCareIdAsync(serviceUser.SocialCareId))
                .ReturnsAsync(serviceUser);

            var act = () => _classUnderTest.ExecuteAsync(request);

            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithMessage($"ServiceUser with ID fakeUserId not found (Parameter 'request')");
            _mockDbSaver.VerifyChangesNotSaved();
        }
    }
}
