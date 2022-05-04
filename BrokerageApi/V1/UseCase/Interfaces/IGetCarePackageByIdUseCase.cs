using System.Threading.Tasks;
using BrokerageApi.V1.Infrastructure;

namespace BrokerageApi.V1.UseCase.Interfaces
{
    public interface IGetCarePackageByIdUseCase
    {
        public Task<CarePackage> ExecuteAsync(int id);
    }
}
