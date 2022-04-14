using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace BrokerageApi.V1.Infrastructure
{
    public class BrokerageContext : DbContext
    {
        static BrokerageContext()
        {
            NpgsqlConnection.GlobalTypeMapper.MapEnum<ReferralStatus>("referral_status");
            NpgsqlConnection.GlobalTypeMapper.MapEnum<WorkflowType>("workflow_type");
            NpgsqlConnection.GlobalTypeMapper.MapEnum<UserRole>("user_role");
        }

        public BrokerageContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Referral> Referrals { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseNpgsql(o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasPostgresEnum<ReferralStatus>();
            modelBuilder.HasPostgresEnum<WorkflowType>();
            modelBuilder.HasPostgresEnum<UserRole>();

            modelBuilder.Entity<Referral>()
                .HasIndex(r => r.WorkflowId)
                .IsUnique();

            modelBuilder.Entity<Service>()
                .Property(s => s.Id)
                .ValueGeneratedNever();

            modelBuilder.Entity<Service>()
                .Property(s => s.IsArchived)
                .HasDefaultValue(false);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            OnBeforeSaving();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override async Task<int> SaveChangesAsync(
            bool acceptAllChangesOnSuccess,
            CancellationToken cancellationToken = default(CancellationToken)
        )
        {
            OnBeforeSaving();
            return (await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken));
        }

        private void OnBeforeSaving()
        {
            var entries = ChangeTracker.Entries();
            var currentTime = DateTime.UtcNow;

            foreach (var entry in entries)
            {
                if (entry.Entity is BaseEntity trackable)
                {
                    switch (entry.State)
                    {
                        case EntityState.Modified:
                            trackable.UpdatedAt = currentTime;
                            break;

                        case EntityState.Added:
                            trackable.CreatedAt = currentTime;
                            trackable.UpdatedAt = currentTime;
                            break;
                    }
                }
            }
        }
    }
}
