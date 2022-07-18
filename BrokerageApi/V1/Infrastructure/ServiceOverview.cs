using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BrokerageApi.V1.Services.Interfaces;
using NodaTime;

namespace BrokerageApi.V1.Infrastructure
{
    public class ServiceOverview
    {
        public ServiceOverview()
        {
        }

        public ServiceOverview(BrokerageContext db)
        {
            Clock = db.Clock;
        }

        [Required]
        public string SocialCareId { get; set; }

        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public LocalDate StartDate { get; set; }

        public LocalDate? EndDate { get; set; }

        public decimal? WeeklyCost { get; set; }

        public decimal? WeeklyPayment { get; set; }

        public decimal? AnnualCost { get; set; }

        public ServiceStatus Status
        {
            get
            {
                if (EndDate == null || EndDate > Today)
                {
                    if (StartDate < Today)
                    {
                        return ServiceStatus.Active;
                    }
                    else
                    {
                        return ServiceStatus.Inactive;
                    }
                }
                else
                {
                    return ServiceStatus.Ended;
                }
            }
        }

        [NotMapped]
        public IClockService Clock { get; set; }

        private LocalDate Today => Clock.Today;
    }
}
