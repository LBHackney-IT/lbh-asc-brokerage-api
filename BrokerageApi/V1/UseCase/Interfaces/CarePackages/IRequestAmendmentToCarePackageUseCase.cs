using System.Threading.Tasks;

namespace BrokerageApi.V1.UseCase.Interfaces.CarePackages
{
    public interface IRequestAmendmentToCarePackageUseCase
    {
        public Task ExecuteAsync(int referralId, string comment);
    }
}
