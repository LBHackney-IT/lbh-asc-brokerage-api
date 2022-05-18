using System;
using System.Threading.Tasks;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.Services;
using BrokerageApi.V1.Services.Interfaces;
using BrokerageApi.V1.UseCase.Interfaces;
using NodaTime;

namespace BrokerageApi.V1.UseCase
{
    public class EndElementUseCase : IEndElementUseCase
    {
        private readonly IElementGateway _elementGateway;
        private readonly IDbSaver _dbSaver;
        private readonly IClockService _clockService;

        public EndElementUseCase(IElementGateway elementGateway, IDbSaver dbSaver, IClockService clockService)
        {
            _elementGateway = elementGateway;
            _dbSaver = dbSaver;
            _clockService = clockService;
        }

        public async Task ExecuteAsync(int id, LocalDate endDate)
        {
            var element = await _elementGateway.GetByIdAsync(id);

            if (element is null)
            {
                throw new ArgumentNullException(nameof(id), $"Element not found {id}");
            }

            if (element.InternalStatus != ElementStatus.Approved)
            {
                throw new InvalidOperationException($"Element {element.Id} is not approved");
            }

            if (element.EndDate != null && element.EndDate < endDate)
            {
                throw new ArgumentException($"Element {element.Id} has an end date before the requested end date");
            }

            element.EndDate = endDate;
            element.UpdatedAt = _clockService.Now;

            await _dbSaver.SaveChangesAsync();
        }
    }
}
