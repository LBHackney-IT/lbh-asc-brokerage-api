using BrokerageApi.V1.Boundary.Response;

namespace BrokerageApi.V1.UseCase.Interfaces
{
    public interface IGetByIdUseCase
    {
        ResponseObject Execute(int id);
    }
}
