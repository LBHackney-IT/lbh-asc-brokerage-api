using System;

namespace BrokerageApi.V1.Infrastructure
{
    public class BaseEntity
    {
        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
