namespace BrokerageApi.V1.Boundary.Response.Interfaces
{
    public interface IAssignmentResponse
    {
        public UserResponse AssignedBroker { get; set; }

        public UserResponse AssignedApprover { get; set; }

        public string AssignedTo { get; }
    }
}
