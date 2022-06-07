using System.Threading.Tasks;
using BrokerageApi.V1.Boundary.Request;
using BrokerageApi.V1.Infrastructure;

namespace BrokerageApi.V1.UseCase.Interfaces.CarePackageElements
{
    public interface ICreateElementUseCase
    {
        public Task<Element> ExecuteAsync(int referralId, CreateElementRequest request);
    }
}
