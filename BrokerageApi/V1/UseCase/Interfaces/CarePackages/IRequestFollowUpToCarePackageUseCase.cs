using System.Threading.Tasks;
using NodaTime;

namespace BrokerageApi.V1.UseCase.Interfaces.CarePackages
{
    public interface IRequestFollowUpToCarePackageUseCase
    {
        public Task ExecuteAsync(int referralId, string comment, LocalDate followUpDate);
    }
}
