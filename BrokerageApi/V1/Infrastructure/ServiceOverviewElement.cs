using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using NodaTime;
using BrokerageApi.V1.Services.Interfaces;
using JetBrains.Annotations;

namespace BrokerageApi.V1.Infrastructure
{
    public class ServiceOverviewElement
    {
        public ServiceOverviewElement()
        {
        }

        public ServiceOverviewElement(BrokerageContext db)
        {
            Clock = db.Clock;
        }

        [Key]
        public int Id { get; set; }

        [Required]
        public string SocialCareId { get; set; }

        public int ReferralId { get; set; }
        public Referral Referral { get; set; }

        public int ServiceId { get; set; }
        public Service Service { get; set; }

        public int? ProviderId { get; set; }
        public Provider Provider { get; set; }

        public ElementTypeType Type { get; set; }

        public string Name { get; set; }

        public LocalDate StartDate { get; set; }

        public LocalDate? EndDate { get; set; }

        public ElementStatus InternalStatus { get; set; }

        public PaymentCycle PaymentCycle { get; set; }

        public decimal? Quantity { get; set; }

        public decimal Cost { get; set; }

        public ElementStatus Status
        {
            get
            {
                if (EndDate.HasValue && Today > EndDate)
                {
                    return ElementStatus.Ended;
                }
                else if (Today >= StartDate)
                {
                    if (Suspensions != null && Suspensions.Any(s => s.Status == ElementStatus.Active))
                    {
                        return ElementStatus.Suspended;
                    }
                    return ElementStatus.Active;
                }
                else
                {
                    return ElementStatus.Inactive;
                }
            }
        }

        public List<ServiceOverviewSuspension> Suspensions { get; set; }

        [NotMapped]
        public IClockService Clock { get; set; }

        private LocalDate Today => Clock.Today;
    }
}
