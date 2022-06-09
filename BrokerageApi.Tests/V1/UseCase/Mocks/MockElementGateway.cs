using System.Threading.Tasks;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;
using Moq;

namespace BrokerageApi.Tests.V1.UseCase.Mocks
{
    public class MockElementGateway : Mock<IElementGateway>
    {
        public Element LastElementAdded { get; set; }

        public MockElementGateway()
        {
            Setup(x => x.AddElementAsync(It.IsAny<Element>()))
                .Callback<Element>((element) =>
                {
                    LastElementAdded = element;
                })
                .Returns(Task.CompletedTask);
        }
    }
}
