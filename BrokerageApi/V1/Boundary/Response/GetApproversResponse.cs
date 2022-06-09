using System.Collections.Generic;

namespace BrokerageApi.V1.Boundary.Response
{
    public class GetApproversResponse
    {
        public decimal EstimatedYearlyCost { get; set; }
        public List<UserResponse> Approvers { get; set; }
    }
}
