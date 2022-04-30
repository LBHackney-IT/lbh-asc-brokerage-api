using System;
using NodaTime;

namespace BrokerageApi.V1.Infrastructure
{
    public class BaseEntity
    {
        public Instant CreatedAt { get; set; }

        public Instant UpdatedAt { get; set; }
    }
}
