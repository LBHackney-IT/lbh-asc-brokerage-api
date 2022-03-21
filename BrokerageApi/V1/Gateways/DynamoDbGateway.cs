using Amazon.DynamoDBv2.DataModel;
using BrokerageApi.V1.Domain;
using BrokerageApi.V1.Factories;
using BrokerageApi.V1.Infrastructure;
using Hackney.Core.Logging;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BrokerageApi.V1.Gateways
{
    public class DynamoDbGateway : IExampleDynamoGateway
    {
        private readonly IDynamoDBContext _dynamoDbContext;
        private readonly ILogger<DynamoDbGateway> _logger;


        public DynamoDbGateway(IDynamoDBContext dynamoDbContext, ILogger<DynamoDbGateway> logger)
        {
            _dynamoDbContext = dynamoDbContext;
            _logger = logger;
        }

        public List<Entity> GetAll()
        {
            return new List<Entity>();
        }

        [LogCall]
        public async Task<Entity> GetEntityById(int id)
        {
            _logger.LogDebug($"Calling IDynamoDBContext.LoadAsync for id parameter {id}");

            var result = await _dynamoDbContext.LoadAsync<DatabaseEntity>(id).ConfigureAwait(false);
            return result?.ToDomain();
        }
    }
}
