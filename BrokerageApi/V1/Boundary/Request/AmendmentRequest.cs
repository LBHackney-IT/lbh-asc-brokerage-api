using System.ComponentModel.DataAnnotations;

namespace BrokerageApi.V1.Boundary.Request
{
    public class AmendmentRequest
    {
        [Required]
        public string Comment { get; set; }
    }
}
