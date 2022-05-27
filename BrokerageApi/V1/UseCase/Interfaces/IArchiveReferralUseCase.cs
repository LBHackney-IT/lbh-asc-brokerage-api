using System.Threading.Tasks;

namespace BrokerageApi.V1.UseCase.Interfaces
{
    public interface IArchiveReferralUseCase
    {
        public Task ExecuteAsync(int referralId, string comment);
    }

}
