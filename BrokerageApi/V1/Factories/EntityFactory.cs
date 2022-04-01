using BrokerageApi.V1.Boundary.Request;
using BrokerageApi.V1.Infrastructure;

namespace BrokerageApi.V1.Factories
{
    public static class EntityFactory
    {
        public static Referral ToDatabase(this CreateReferralRequest request)
        {
            return new Referral
            {
                WorkflowId = request.WorkflowId,
                WorkflowType = request.WorkflowType,
                FormName = request.FormName,
                SocialCareId = request.SocialCareId,
                ResidentName = request.ResidentName,
                UrgentSince = request.UrgentSince,
                Status = ReferralStatus.Unassigned
            };
        }
    }
}
