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
                PrimarySupportReason = request.PrimarySupportReason,
                DirectPayments = request.DirectPayments,
                UrgentSince = request.UrgentSince,
                Status = ReferralStatus.Unassigned,
                Note = request.Note
            };
        }

        public static Element ToDatabase(this CreateElementRequest request)
        {
            return new Element
            {
                ElementTypeId = request.ElementTypeId,
                NonPersonalBudget = request.NonPersonalBudget,
                ProviderId = request.ProviderId,
                Details = request.Details,
                ParentElementId = request.ParentElementId,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Monday = request.Monday,
                Tuesday = request.Tuesday,
                Wednesday = request.Wednesday,
                Thursday = request.Thursday,
                Friday = request.Friday,
                Saturday = request.Saturday,
                Sunday = request.Sunday,
                Quantity = request.Quantity,
                Cost = request.Cost
            };
        }

        public static Element ToDatabase(this EditElementRequest request, Element existingElement)
        {
            existingElement.ElementTypeId = request.ElementTypeId;
            existingElement.NonPersonalBudget = request.NonPersonalBudget;
            existingElement.ProviderId = request.ProviderId;
            existingElement.Details = request.Details;
            existingElement.StartDate = request.StartDate;
            existingElement.EndDate = request.EndDate;
            existingElement.Monday = request.Monday;
            existingElement.Tuesday = request.Tuesday;
            existingElement.Wednesday = request.Wednesday;
            existingElement.Thursday = request.Thursday;
            existingElement.Friday = request.Friday;
            existingElement.Saturday = request.Saturday;
            existingElement.Sunday = request.Sunday;
            existingElement.Quantity = request.Quantity;
            existingElement.Cost = request.Cost;

            return existingElement;
        }

        public static Element ToDatabase(this CreateCareChargeRequest request)
        {
            return new Element
            {
                ElementTypeId = request.ElementTypeId,
                ParentElementId = request.ParentElementId,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Monday = request.Monday,
                Tuesday = request.Tuesday,
                Wednesday = request.Wednesday,
                Thursday = request.Thursday,
                Friday = request.Friday,
                Saturday = request.Saturday,
                Sunday = request.Sunday,
                Quantity = request.Quantity,
                Cost = request.Cost
            };
        }

        public static Element ToDatabase(this EditCareChargeRequest request, Element existingElement)
        {
            existingElement.ElementTypeId = request.ElementTypeId;
            existingElement.StartDate = request.StartDate;
            existingElement.EndDate = request.EndDate;
            existingElement.Monday = request.Monday;
            existingElement.Tuesday = request.Tuesday;
            existingElement.Wednesday = request.Wednesday;
            existingElement.Thursday = request.Thursday;
            existingElement.Friday = request.Friday;
            existingElement.Saturday = request.Saturday;
            existingElement.Sunday = request.Sunday;
            existingElement.Quantity = request.Quantity;
            existingElement.Cost = request.Cost;

            return existingElement;
        }
    }
}
