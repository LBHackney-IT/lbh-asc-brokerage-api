
using System.Collections.Generic;
using System.Threading.Tasks;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.Controllers.Parameters;


namespace BrokerageApi.V1.UseCase.Interfaces.ServiceUsers
{
    public interface IGetServiceUserByRequestUseCase
    {
        public Task<IEnumerable<ServiceUser>> ExecuteAsync(GetServiceUserRequest request);
    }

}
