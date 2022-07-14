using NodaTime;

namespace BrokerageApi.V1.Controllers.Parameters
{
    public class GetServiceUserRequest
    {
        public string SocialCareId { get; set; }

        public string ServiceUserName { get; set; }

        public LocalDate? DateOfBirth { get; set; }
        public int? Provider { get; set; }

    }

}
