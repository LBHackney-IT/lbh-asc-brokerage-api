using System;
using System.ComponentModel.DataAnnotations;
using BrokerageApi.V1.Infrastructure;

namespace BrokerageApi.V1.Boundary.Request
{
    public class AssignBrokerRequest
    {
        [Required]
        public string Broker { get; set; }
    }
}
