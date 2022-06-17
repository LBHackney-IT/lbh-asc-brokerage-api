using System.ComponentModel.DataAnnotations;
using NodaTime;


namespace BrokerageApi.V1.Infrastructure
{
    public class ServiceUser : BaseEntity
    {
        [Key]
        public string SocialCareId { get; set; }

        [Required]
        public string ServiceUserName { get; set; }

        [Required]
        public LocalDate DateOfBirth { get; set; }


    }
}
