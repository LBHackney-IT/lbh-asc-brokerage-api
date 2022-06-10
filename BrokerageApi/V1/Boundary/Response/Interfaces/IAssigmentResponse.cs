namespace BrokerageApi.V1.Boundary.Response.Interfaces
{
    public interface IAssigmentResponse
    {
        public UserResponse AssignedBroker { get; set; }

        public UserResponse AssignedApprover { get; set; }

        public string AssignedTo { get; }
    }
}
