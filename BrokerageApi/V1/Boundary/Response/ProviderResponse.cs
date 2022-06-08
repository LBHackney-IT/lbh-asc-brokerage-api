using Newtonsoft.Json;
using BrokerageApi.V1.Infrastructure;

namespace BrokerageApi.V1.Boundary.Response
{
    public class ProviderResponse
    {
        public int Id { get; set; }

        [JsonProperty(Required = Required.DisallowNull)]
        public string Name { get; set; }

        [JsonProperty(Required = Required.DisallowNull)]
        public string Address { get; set; }

        public string CedarNumber { get; set; }

        public string CedarSite { get; set; }

        public ProviderType Type { get; set; }
    }
}
