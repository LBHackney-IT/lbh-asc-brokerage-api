using System.Collections.Generic;
using System.Linq;
using Amazon.Runtime.Internal;
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
                IsResidential = carePackage.IsResidential,
                CareChargeStatus = carePackage.CareChargeStatus,
                CareChargesConfirmedAt = carePackage.CareChargesConfirmedAt,
                CreatedAt = carePackage.CreatedAt,
                UpdatedAt = carePackage.UpdatedAt,
                StartDate = carePackage.StartDate,
                WeeklyCost = carePackage.WeeklyCost,
                WeeklyPayment = carePackage.WeeklyPayment,
                OneOffPayment = carePackage.OneOffPayment,
                EstimatedYearlyCost = carePackage.EstimatedYearlyCost,
                Elements = carePackage.ReferralElements?.Select(re => re.Element.ToResponse(re.ReferralId)).ToList(),
                Comment = carePackage.Comment,
                Amendments = carePackage.ReferralAmendments?.Select(a => a.ToResponse()).ToList(),
                FollowUps = carePackage.ReferralFollowUps?.Select(f => f.ToResponse()).ToList(),
                Workflows = carePackage.Workflows?.Select(w => w.ToResponse()).ToList()
            };
        }

        public static List<CarePackageResponse> ToResponse(this List<CarePackage> carePackages)
        {
            var carePackageResponses = new List<CarePackageResponse>();
            foreach (CarePackage carePackage in carePackages)
            {
                carePackageResponses.Add(carePackage.ToResponse());
            }
            return carePackageResponses;
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
                SuspensionElements = element.SuspensionElements?.Select(e => e.ToResponse(referralId)).ToList(),
                Comment = element.Comment,
                PendingEndDate = referralElement?.PendingEndDate,
                PendingCancellation = referralElement?.PendingCancellation,
                PendingComment = referralElement?.PendingComment,
                IsSuspension = element.IsSuspension
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
                PaymentCycle = elementType.PaymentCycle,
                NonPersonalBudget = elementType.NonPersonalBudget,
                IsS117 = elementType.IsS117,
                IsResidential = elementType.IsResidential,
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
                Note = referral.Note,
                Comment = referral.Comment,
                StartedAt = referral.StartedAt,
                IsResidential = referral.IsResidential,
                CareChargeStatus = referral.CareChargeStatus,
                CareChargesConfirmedAt = referral.CareChargesConfirmedAt,
                CreatedAt = referral.CreatedAt,
                UpdatedAt = referral.UpdatedAt,
                AssignedBroker = referral.AssignedBroker?.ToResponse(),
                AssignedApprover = referral.AssignedApprover?.ToResponse(),
                Amendments = referral.ReferralAmendments?.Select(a => a.ToResponse()).ToList(),
                FollowUps = referral.ReferralFollowUps?.Select(f => f.ToResponse()).ToList(),
                Workflows = referral.Workflows?.Select(w => w.ToResponse()).ToList()
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

        public static ServiceOverviewResponse ToResponse(this ServiceOverview serviceOverview)
        {
            return new ServiceOverviewResponse
            {
                Id = serviceOverview.Id,
                Name = serviceOverview.Name,
                StartDate = serviceOverview.StartDate,
                EndDate = serviceOverview.EndDate,
                WeeklyCost = serviceOverview.WeeklyCost,
                WeeklyPayment = serviceOverview.WeeklyPayment,
                AnnualCost = serviceOverview.AnnualCost,
                Status = serviceOverview.Status,
                Elements = serviceOverview.Elements?.Select(e => e.ToResponse()).ToList()
            };
        }

        public static ServiceOverviewElementResponse ToResponse(this ServiceOverviewElement element)
        {
            return new ServiceOverviewElementResponse
            {
                Id = element.Id,
                Type = element.Type,
                Name = element.Name,
                Referral = element.Referral?.ToResponse(),
                Provider = element.Provider?.ToResponse(),
                StartDate = element.StartDate,
                EndDate = element.EndDate,
                Status = element.Status,
                PaymentCycle = element.PaymentCycle,
                Quantity = element.Quantity,
                Cost = element.Cost,
                Suspensions = element.Suspensions?.Select(s => s.ToResponse()).ToList()
            };
        }

        public static ServiceOverviewSuspensionResponse ToResponse(this ServiceOverviewSuspension element)
        {
            return new ServiceOverviewSuspensionResponse
            {
                Id = element.Id,
                Referral = element.Referral?.ToResponse(),
                StartDate = element.StartDate,
                EndDate = element.EndDate,
                Status = element.Status,
                Quantity = element.Quantity,
                Cost = element.Cost
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
                UpdatedAt = user.UpdatedAt,
                ApprovalLimit = user.ApprovalLimit
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

        public static AmendmentResponse ToResponse(this ReferralAmendment amendment)
        {
            return new AmendmentResponse
            {
                Comment = amendment.Comment,
                Status = amendment.Status,
                RequestedAt = amendment.RequestedAt
            };
        }

        public static FollowUpResponse ToResponse(this ReferralFollowUp followUp)
        {
            return new FollowUpResponse
            {
                Comment = followUp.Comment,
                Date = followUp.Date,
                Status = followUp.Status,
                RequestedAt = followUp.RequestedAt,
                RequestedBy = followUp.RequestedBy?.ToResponse()
            };
        }

        public static ServiceUserResponse ToResponse(this ServiceUser serviceUser)
        {
            return new ServiceUserResponse
            {
                SocialCareId = serviceUser.SocialCareId,
                ServiceUserName = serviceUser.ServiceUserName,
                DateOfBirth = serviceUser.DateOfBirth,
                CarePackages = serviceUser.CarePackages?.ToResponse(),
                CedarNumber = serviceUser.CedarNumber,
                CreatedAt = serviceUser.CreatedAt,
                UpdatedAt = serviceUser.UpdatedAt
            };
        }

        public static WorkflowResponse ToResponse(this Workflow workflow)
        {
            return new WorkflowResponse
            {
                Id = workflow.Id,
                WorkflowType = workflow.WorkflowType,
                FormName = workflow.FormName,
                Note = workflow.Note,
                PrimarySupportReason = workflow.PrimarySupportReason,
                DirectPayments = workflow.DirectPayments,
                UrgentSince = workflow.UrgentSince,
                CreatedAt = workflow.CreatedAt
            };
        }
    }
}
