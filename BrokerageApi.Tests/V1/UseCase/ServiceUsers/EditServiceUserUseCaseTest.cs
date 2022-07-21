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
            var serviceUsers = _fixture.BuildServiceUser().CreateMany();
            var serviceUserRequest = _fixture.BuildServiceUserRequest(serviceUsers.ElementAt(0).SocialCareId).Create();

            var request = _fixture.BuildEditServiceUserRequest(serviceUsers.ElementAt(0).SocialCareId).Create();

            _mockServiceUserGateway
                .Setup(x => x.GetByRequestAsync(serviceUserRequest))
                .ReturnsAsync(serviceUsers);            

            
            //Act
            var result = await _classUnderTest.ExecuteAsync(request);

            //Assert
            result.Should().BeEquivalentTo(request.ToDatabase(serviceUsers.ElementAt(0)));

        }    
    }
}    