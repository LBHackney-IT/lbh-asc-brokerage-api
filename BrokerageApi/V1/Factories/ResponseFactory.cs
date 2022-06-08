using System.Linq;
using BrokerageApi.V1.Boundary.Response;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.Infrastructure.AuditEvents;
using Newtonsoft.Json.Linq;
using X.PagedList;

namespace BrokerageApi.V1.Factories
{
    public static class ResponseFactory
    {
        public static CarePackageResponse ToResponse(this CarePackage carePackage)
        {
            return new CarePackageResponse
            {
                Id = carePackage.Id,
                WorkflowId = carePackage.WorkflowId,
                WorkflowType = carePackage.WorkflowType,
                FormName = carePackage.FormName,
                SocialCareId = carePackage.SocialCareId,
                ResidentName = carePackage.ResidentName,
                PrimarySupportReason = carePackage.PrimarySupportReason,
                UrgentSince = carePackage.UrgentSince,
                CarePackageName = carePackage.CarePackageName,
                Status = carePackage.Status,
                AssignedBroker = carePackage.AssignedBroker?.ToResponse(),
                AssignedApprover = carePackage.AssignedApprover?.ToResponse(),
                Note = carePackage.Note,
                StartedAt = carePackage.StartedAt,
                CreatedAt = carePackage.CreatedAt,
                UpdatedAt = carePackage.UpdatedAt,
                StartDate = carePackage.StartDate,
                WeeklyCost = carePackage.WeeklyCost,
                WeeklyPayment = carePackage.WeeklyPayment,
                Elements = carePackage.ReferralElements.Select(re => re.Element.ToResponse(re.ReferralId)).ToList(),
                Comment = carePackage.Comment
            };
        }

        public static ElementResponse ToResponse(this Element element, int? referralId = null, bool includeParent = true)
        {
            var referralElement = referralId.HasValue ? element.ReferralElements?.SingleOrDefault(re => re.ReferralId == referralId) : null;

            return new ElementResponse
            {
                Id = element.Id,
                ElementType = element.ElementType?.ToResponse(),
                NonPersonalBudget = element.NonPersonalBudget,
                Provider = element.Provider?.ToResponse(),
                Details = element.Details,
                Status = element.Status,
                StartDate = element.StartDate,
                EndDate = element.EndDate,
                Monday = element.Monday,
                Tuesday = element.Tuesday,
                Wednesday = element.Wednesday,
                Thursday = element.Thursday,
                Friday = element.Friday,
                Saturday = element.Saturday,
                Sunday = element.Sunday,
                Quantity = element.Quantity,
                Cost = element.Cost,
                CreatedBy = element.CreatedBy,
                CreatedAt = element.CreatedAt,
                UpdatedAt = element.UpdatedAt,
                ParentElement = includeParent ? element.ParentElement?.ToResponse(referralId, false) : null,
                SuspensionElements = element.SuspensionElements?.Select(e => e.ToResponse()).ToList(),
                Comment = element.Comment,
                PendingEndDate = referralElement?.PendingEndDate,
                PendingCancellation = referralElement?.PendingCancellation,
                PendingComment = referralElement?.PendingComment
            };
        }

        public static ElementTypeResponse ToResponse(this ElementType elementType)
        {
            return new ElementTypeResponse
            {
                Id = elementType.Id,
                Name = elementType.Name,
                Type = elementType.Type,
                CostType = elementType.CostType,
                Billing = elementType.Billing,
                NonPersonalBudget = elementType.NonPersonalBudget,
                Service = elementType.Service != null
                    ? new ServiceResponse
                    {
                        Id = elementType.Service.Id,
                        ParentId = elementType.Service.ParentId,
                        Name = elementType.Service.Name,
                        Description = elementType.Service.Description,
                        HasProvisionalClientContributions = elementType.Service.HasProvisionalClientContributions
                    }
                    : null
            };
        }

        public static ProviderResponse ToResponse(this Provider provider)
        {
            return new ProviderResponse
            {
                Id = provider.Id,
                Name = provider.Name,
                Address = provider.Address,
                CedarNumber = provider.CedarNumber,
                CedarSite = provider.CedarSite,
                Type = provider.Type
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
                PrimarySupportReason = referral.PrimarySupportReason,
                DirectPayments = referral.DirectPayments,
                UrgentSince = referral.UrgentSince,
                Status = referral.Status,
                AssignedBroker = referral.AssignedBroker,
                Note = referral.Note,
                StartedAt = referral.StartedAt,
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
                HasProvisionalClientContributions = service.HasProvisionalClientContributions,
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

        public static AuditEventResponse ToResponse(this AuditEvent auditEvent)
        {
            return new AuditEventResponse
            {
                Id = auditEvent.Id,
                Message = auditEvent.Message,
                CreatedAt = auditEvent.CreatedAt,
                EventType = auditEvent.EventType,
                UserId = auditEvent.UserId,
                SocialCareId = auditEvent.SocialCareId,
                Metadata = JObject.Parse(auditEvent.Metadata),
                ReferralId = auditEvent.Referral?.Id,
                FormName = auditEvent.Referral?.FormName
            };
        }

        public static PageMetadataResponse ToResponse(this IPagedList pagedListMetaData)
        {
            return new PageMetadataResponse
            {
                PageCount = pagedListMetaData.PageCount,
                TotalItemCount = pagedListMetaData.TotalItemCount,
                PageNumber = pagedListMetaData.PageNumber,
                PageSize = pagedListMetaData.PageSize,
                HasPreviousPage = pagedListMetaData.HasPreviousPage,
                HasNextPage = pagedListMetaData.HasNextPage,
                IsFirstPage = pagedListMetaData.IsFirstPage,
                IsLastPage = pagedListMetaData.IsLastPage,
                FirstItemOnPage = pagedListMetaData.FirstItemOnPage,
                LastItemOnPage = pagedListMetaData.LastItemOnPage
            };
        }
    }
}
