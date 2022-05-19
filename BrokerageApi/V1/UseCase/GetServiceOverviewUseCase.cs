using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.UseCase.Interfaces;

namespace BrokerageApi.V1.UseCase
{
    public class GetServiceOverviewUseCase : IGetServiceOverviewUseCase
    {
        private readonly IElementGateway _elementGateway;

        public GetServiceOverviewUseCase(IElementGateway elementGateway)
        {
            _elementGateway = elementGateway;
        }

        public async Task<IEnumerable<Element>> ExecuteAsync(string socialCareId)
        {
            var elements = await _elementGateway.GetBySocialCareId(socialCareId);

            if (!elements.Any())
            {
                throw new ArgumentException($"Service overview not found for: {socialCareId}");
            }

            return elements;
        }
    }
}
