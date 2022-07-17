using System.ComponentModel.DataAnnotations;
using NodaTime;

namespace BrokerageApi.V1.Boundary.Request
{
    public class FollowUpRequest
    {
        [Required]
        public string Comment { get; set; }

        public LocalDate Date { get; set; }
    }
}
