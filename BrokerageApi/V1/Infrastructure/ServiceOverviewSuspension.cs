using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using NodaTime;
using BrokerageApi.V1.Services.Interfaces;
using JetBrains.Annotations;

namespace BrokerageApi.V1.Infrastructure
{
    public class ServiceOverviewSuspension
    {
        public ServiceOverviewSuspension()
        {
        }

        public ServiceOverviewSuspension(BrokerageContext db)
        {
            Clock = db.Clock;
        }

        [Key]
        public int Id { get; set; }

        public int SuspendedElementId { get; set; }

        public int ReferralId { get; set; }
        public Referral Referral { get; set; }

        public LocalDate StartDate { get; set; }

        public LocalDate? EndDate { get; set; }

        public ElementStatus InternalStatus { get; set; }

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
                    return ElementStatus.Active;
                }
                else
                {
                    return ElementStatus.Inactive;
                }
            }
        }

        [NotMapped]
        public IClockService Clock { get; set; }

        private LocalDate Today => Clock.Today;
    }
}
