using System.Collections.Generic;
using BrokerageApi.V1.Domain;

namespace BrokerageApi.V1.Gateways
{
    public interface IExampleGateway
    {
        Entity GetEntityById(int id);

        List<Entity> GetAll();
    }
}
