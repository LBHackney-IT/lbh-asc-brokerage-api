using System.Collections.Generic;
using System.Threading.Tasks;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.UseCase.Interfaces;

namespace BrokerageApi.V1.UseCase
{
    public class GetCurrentElementsUseCase : IGetCurrentElementsUseCase
    {
        private readonly IElementGateway _elementGateway;

        public GetCurrentElementsUseCase(IElementGateway elementGateway)
        {
            _elementGateway = elementGateway;
        }

        public async Task<IEnumerable<Element>> ExecuteAsync()
        {
            return await _elementGateway.GetCurrentAsync();
        }
    }
}
