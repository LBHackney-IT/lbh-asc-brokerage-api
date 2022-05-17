using System.ComponentModel.DataAnnotations;

namespace BrokerageApi.V1.Boundary.Request
{
    public class AssignBrokerRequest
    {
        [Required]
        public string Broker { get; set; }
    }
}
