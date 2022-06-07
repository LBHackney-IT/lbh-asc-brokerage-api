using System.ComponentModel.DataAnnotations;

namespace BrokerageApi.V1.Boundary.Request
{
    public class ArchiveReferralRequest
    {
        [Required]
        public string Comment { get; set; }
    }
}
