using System.Threading.Tasks;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.Boundary.Request;


namespace BrokerageApi.V1.UseCase.Interfaces
{
    public interface IGetServiceUserByRequestUseCase
    {
        public Task<ServiceUser> ExecuteAsync(GetServiceUserRequest request);
    }

}