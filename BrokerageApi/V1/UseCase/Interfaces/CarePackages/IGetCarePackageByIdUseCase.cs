using System.Threading.Tasks;

namespace BrokerageApi.V1.UseCase.Interfaces.CarePackages
{
    public interface IGetCarePackageByIdUseCase
    {
        public Task<Infrastructure.CarePackage> ExecuteAsync(int id);
    }
}
