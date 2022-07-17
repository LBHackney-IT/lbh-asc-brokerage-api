using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using NodaTime;
using BrokerageApi.V1.Services.Interfaces;
using JetBrains.Annotations;

namespace BrokerageApi.V1.Infrastructure
{
    public class Element : BaseEntity
    {
        private IClockService _clock;

        public Element()
        {
        }

        public Element(BrokerageContext db)
        {
            _clock = db.Clock;
        }
        public Element(Element element)
        {
            SocialCareId = element.SocialCareId;
            ElementTypeId = element.ElementTypeId;
            NonPersonalBudget = element.NonPersonalBudget;
            ProviderId = element.ProviderId;
            Details = element.Details;
            Monday = element.Monday;
            Tuesday = element.Tuesday;
            Wednesday = element.Wednesday;
            Thursday = element.Thursday;
            Friday = element.Friday;
            Saturday = element.Saturday;
            Sunday = element.Sunday;
            Quantity = element.Quantity;
            Cost = element.Cost;
            CostCentre = element.CostCentre;
        }

        [Key]
        public int Id { get; set; }

        [Required]
        public string SocialCareId { get; set; }

        public int ElementTypeId { get; set; }
        public ElementType ElementType { get; set; }

        public bool NonPersonalBudget { get; set; }

        public int? ProviderId { get; set; }
        public Provider Provider { get; set; }

        public string Details { get; set; }

        public ElementStatus InternalStatus { get; set; }

        public int? ParentElementId { get; set; }
        public Element ParentElement { get; set; }
        public List<Element> ChildElements { get; set; }

        public int? SuspendedElementId { get; set; }
        public Element SuspendedElement { get; set; }
        [CanBeNull]
        public List<Element> SuspensionElements { get; set; }

        public bool IsSuspension { get; set; }

        public string CreatedBy { get; set; }

        public LocalDate StartDate { get; set; }

        public LocalDate? EndDate { get; set; }

        [Column(TypeName = "jsonb")]
        public ElementCost? Monday { get; set; }

        [Column(TypeName = "jsonb")]
        public ElementCost? Tuesday { get; set; }

        [Column(TypeName = "jsonb")]
        public ElementCost? Wednesday { get; set; }

        [Column(TypeName = "jsonb")]
        public ElementCost? Thursday { get; set; }

        [Column(TypeName = "jsonb")]
        public ElementCost? Friday { get; set; }

        [Column(TypeName = "jsonb")]
        public ElementCost? Saturday { get; set; }

        [Column(TypeName = "jsonb")]
        public ElementCost? Sunday { get; set; }

        public decimal? Quantity { get; set; }

        public decimal Cost { get; set; }

        public string CostCentre { get; set; }

        public string Comment { get; set; }

        public List<decimal> DailyCosts { get; set; }

        public List<ReferralElement> ReferralElements { get; set; }

        public List<Referral> Referrals { get; set; }

        public List<CarePackage> CarePackages { get; set; }

        public ElementStatus Status
        {
            get
            {
                switch (InternalStatus)
                {
                    case ElementStatus.Approved:
                        if (EndDate.HasValue && Today > EndDate)
                        {
                            return ElementStatus.Ended;
                        }
                        else if (Today >= StartDate)
                        {
                            if (SuspensionElements != null && SuspensionElements.Any(e => e.Status == ElementStatus.Active))
                            {
                                return ElementStatus.Suspended;
                            }
                            return ElementStatus.Active;
                        }
                        else
                        {
                            return ElementStatus.Inactive;
                        }

                    default:
                        return InternalStatus;
                }
            }
        }

        public bool IsResidential =>
            ElementType is { IsResidential: true } &&
            InternalStatus == ElementStatus.Approved;

        private LocalDate Today
        {
            get => _clock.Today;
        }
    }
}
