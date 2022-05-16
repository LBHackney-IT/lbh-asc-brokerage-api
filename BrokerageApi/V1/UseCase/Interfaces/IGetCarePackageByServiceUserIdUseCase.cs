using System.Collections.Generic;
using System.Threading.Tasks;
using BrokerageApi.V1.Infrastructure;

namespace BrokerageApi.V1.UseCase.Interfaces
{
    public interface IGetCarePackagesByServiceUserIdUseCase
    {
        public Task<IEnumerable<CarePackage>> ExecuteAsync(int serviceUserId);
    }
}
