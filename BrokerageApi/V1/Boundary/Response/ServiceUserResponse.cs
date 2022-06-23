using NodaTime;

namespace BrokerageApi.V1.Boundary.Response
{
    public class ServiceUserResponse
    {
        public string SocialCareId { get; set; }

        public string ServiceUserName { get; set; }

        public LocalDate DateOfBirth { get; set; }

        public Instant CreatedAt { get; set; }

        public Instant UpdatedAt { get; set; }

    }
}
