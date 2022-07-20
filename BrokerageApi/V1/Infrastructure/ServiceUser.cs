using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NodaTime;
using NpgsqlTypes;



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

        public NpgsqlTsVector NameSearchVector { get; set; }

        public List<CarePackage> CarePackages { get; set; }


    }
}
