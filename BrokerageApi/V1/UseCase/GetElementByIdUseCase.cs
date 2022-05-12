using System;
using System.Threading.Tasks;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.UseCase.Interfaces;

namespace BrokerageApi.V1.UseCase
{
    public class GetElementByIdUseCase : IGetElementByIdUseCase
    {
        private readonly IElementGateway _elementGateway;

        public GetElementByIdUseCase(IElementGateway elementGateway)
        {
            _elementGateway = elementGateway;
        }

        public async Task<Element> ExecuteAsync(int id)
        {
            var element = await _elementGateway.GetByIdAsync(id);

            if (element is null)
            {
                throw new ArgumentNullException(nameof(id), $"Element not found for: {id}");
            }

            return element;
        }
    }
}
