using System.Threading.Tasks;
using BrokerageApi.V1.Boundary.Request;
using BrokerageApi.V1.Infrastructure;

namespace BrokerageApi.V1.UseCase.Interfaces.ServiceUsers
{
    public interface IEditServiceUserUseCase
    {
        public Task<ServiceUser> ExecuteAsync(EditServiceUserRequest request);
    }
}
