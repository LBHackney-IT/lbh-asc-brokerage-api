using System.Threading.Tasks;
using NodaTime;

namespace BrokerageApi.V1.UseCase.Interfaces.CarePackages
{
    public interface IEndCarePackageUseCase
    {
        public Task ExecuteAsync(int referralId, LocalDate endDate, string comment);
    }

}
