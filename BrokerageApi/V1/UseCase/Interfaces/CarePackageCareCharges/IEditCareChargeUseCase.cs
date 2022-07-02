using System.Threading.Tasks;
using BrokerageApi.V1.Boundary.Request;
using BrokerageApi.V1.Infrastructure;

namespace BrokerageApi.V1.UseCase.Interfaces.CarePackageCareCharges
{
    public interface IEditCareChargeUseCase
    {
        public Task<Element> ExecuteAsync(int referralId, int elementId, EditCareChargeRequest request);
    }
}
