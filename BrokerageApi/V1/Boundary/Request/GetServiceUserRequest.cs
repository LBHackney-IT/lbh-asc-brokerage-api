using NodaTime;

namespace BrokerageApi.V1.Boundary.Request
{
    public class GetServiceUserRequest
    {
        public string SocialCareId { get; set; }

        public string ServiceUserName { get; set; }

        public LocalDate DateOfBirth { get; set; }
    }

}
