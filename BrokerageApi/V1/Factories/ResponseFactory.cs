using System.Collections.Generic;
using System.Linq;
using BrokerageApi.V1.Boundary.Response;
using BrokerageApi.V1.Infrastructure;

namespace BrokerageApi.V1.Factories
{
    public static class ResponseFactory
    {
        public static ElementTypeResponse ToResponse(this ElementType elementType)
        {
            return new ElementTypeResponse
            {
                Id = elementType.Id,
                Name = elementType.Name,
                CostType = elementType.CostType,
                NonPersonalBudget = elementType.NonPersonalBudget
            };
        }

        public static ReferralResponse ToResponse(this Referral referral)
        {
            return new ReferralResponse
            {
                Id = referral.Id,
                WorkflowId = referral.WorkflowId,
                WorkflowType = referral.WorkflowType,
                FormName = referral.FormName,
                SocialCareId = referral.SocialCareId,
                ResidentName = referral.ResidentName,
                UrgentSince = referral.UrgentSince,
                Status = referral.Status,
                AssignedTo = referral.AssignedTo,
                Note = referral.Note,
                CreatedAt = referral.CreatedAt,
                UpdatedAt = referral.UpdatedAt
            };
        }

        public static ServiceResponse ToResponse(this Service service)
        {
            return new ServiceResponse
            {
                Id = service.Id,
                ParentId = service.ParentId,
                Name = service.Name,
                Description = service.Description,
                ElementTypes = service.ElementTypes?.Select(et => et.ToResponse()).ToList()
            };
        }

        public static UserResponse ToResponse(this User user)
        {
            return new UserResponse
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Roles = user.Roles,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };
        }
    }
}
