using System.Collections.Generic;
using Newtonsoft.Json;
using NodaTime;
using BrokerageApi.V1.Infrastructure;

namespace BrokerageApi.V1.Boundary.Response
{
    public class UserResponse
    {
        public int Id { get; set; }

        [JsonProperty(Required = Required.DisallowNull)]
        public string Name { get; set; }

        [JsonProperty(Required = Required.DisallowNull)]
        public string Email { get; set; }

        public List<UserRole> Roles { get; set; }

        public bool IsActive { get; set; }

        public Instant CreatedAt { get; set; }

        public Instant UpdatedAt { get; set; }
    }
}
