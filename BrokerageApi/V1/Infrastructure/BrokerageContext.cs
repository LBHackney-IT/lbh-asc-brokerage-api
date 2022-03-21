using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace BrokerageApi.V1.Infrastructure
{
    public class BrokerageContext : DbContext
    {
        public BrokerageContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseNpgsql(o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery));
        }
    }
}
