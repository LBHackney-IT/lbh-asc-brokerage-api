using System.Threading.Tasks;
using NodaTime;

namespace BrokerageApi.V1.UseCase.Interfaces.CarePackages
{
    public interface ISuspendCarePackageUseCase
    {
        public Task ExecuteAsync(int referralId, LocalDate startDate, LocalDate endDate, string comment);
    }

}
