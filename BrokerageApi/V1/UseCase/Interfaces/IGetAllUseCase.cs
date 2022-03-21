using BrokerageApi.V1.Boundary.Response;

namespace BrokerageApi.V1.UseCase.Interfaces
{
    public interface IGetAllUseCase
    {
        ResponseObjectList Execute();
    }
}
