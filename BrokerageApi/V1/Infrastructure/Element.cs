using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NodaTime;
using BrokerageApi.V1.Services.Interfaces;

namespace BrokerageApi.V1.Infrastructure
{
    public class Element : BaseEntity
    {
        private IClockService _clock;

        public Element()
        {
        }

        private Element(BrokerageContext db)
        {
            _clock = db.Clock;
        }

        [Key]
        public int Id { get; set; }

        [Required]
        public string SocialCareId { get; set; }

        public int ElementTypeId { get; set; }
        public ElementType ElementType { get; set; }

        public bool NonPersonalBudget { get; set; }

        public int ProviderId { get; set; }
        public Provider Provider { get; set; }

        [Required]
        public string Details { get; set; }

        public ElementStatus InternalStatus { get; set; }

        public int? ParentElementId { get; set; }
        public Element ParentElement { get; set; }
        public List<Element> ChildElements { get; set; }

        public int? SuspendedElementId { get; set; }
        public Element SuspendedElement { get; set; }
        public List<Element> SuspensionElements { get; set; }

        public bool IsSuspension { get; set; }

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

        private LocalDate Today
        {
            get => _clock.Today;
        }
    }
}
