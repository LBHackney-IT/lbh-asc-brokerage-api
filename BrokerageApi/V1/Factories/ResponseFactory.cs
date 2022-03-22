using System.Collections.Generic;
using System.Linq;
using BrokerageApi.V1.Boundary.Response;
using BrokerageApi.V1.Infrastructure;

namespace BrokerageApi.V1.Factories
{
    public static class ResponseFactory
    {
        public static ReferralResponse ToResponse(this Referral referral)
        {
            return new ReferralResponse
            {
                Id = referral.Id,
                WorkflowId = referral.WorkflowId,
                WorkflowType = referral.WorkflowType,
                SocialCareId = referral.SocialCareId,
                Name = referral.Name,
                Status = referral.Status,
                AssignedTo = referral.AssignedTo,
                CreatedAt = referral.CreatedAt,
                UpdatedAt = referral.UpdatedAt
            };
        }
    }
}
