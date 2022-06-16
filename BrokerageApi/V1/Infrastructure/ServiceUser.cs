using System.ComponentModel.DataAnnotations;
using NodaTime;


namespace BrokerageApi.V1.Infrastructure
{
    public class ServiceUser : BaseEntity
    {
        [Key]
        public int MosaicId { get; set; }

        [Required]
        public string ServiceUserName { get; set; }

        [Required]
        public Instant DateOfBirth { get; set; }


    }
}
