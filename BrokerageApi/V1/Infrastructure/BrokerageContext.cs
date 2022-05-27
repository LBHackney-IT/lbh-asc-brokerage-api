using System.Threading;
using System.Threading.Tasks;
using BrokerageApi.V1.Infrastructure.AuditEvents;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Npgsql;
using BrokerageApi.V1.Services.Interfaces;

namespace BrokerageApi.V1.Infrastructure
{
    public class BrokerageContext : DbContext
    {
        private readonly IClockService _clock;

        static BrokerageContext()
        {
            NpgsqlConnection.GlobalTypeMapper.UseNodaTime();

            NpgsqlConnection.GlobalTypeMapper.MapEnum<ElementCostType>("element_cost_type");
            NpgsqlConnection.GlobalTypeMapper.MapEnum<ElementStatus>("element_status");
            NpgsqlConnection.GlobalTypeMapper.MapEnum<ProviderType>("provider_type");
            NpgsqlConnection.GlobalTypeMapper.MapEnum<ReferralStatus>("referral_status");
            NpgsqlConnection.GlobalTypeMapper.MapEnum<WorkflowType>("workflow_type");
            NpgsqlConnection.GlobalTypeMapper.MapEnum<UserRole>("user_role");
            NpgsqlConnection.GlobalTypeMapper.MapEnum<AuditEventType>("audit_event_type");
        }

        public BrokerageContext(DbContextOptions options, IClockService clock) : base(options)
        {
            _clock = clock;
        }

        public IClockService Clock
        {
            get => _clock;
        }

        public DbSet<CarePackage> CarePackages { get; set; }
        public DbSet<Element> Elements { get; set; }
        public DbSet<ElementType> ElementTypes { get; set; }
        public DbSet<Provider> Providers { get; set; }
        public DbSet<ProviderService> ProviderServices { get; set; }
        public DbSet<Referral> Referrals { get; set; }
        public DbSet<ReferralElement> ReferralElements { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<AuditEvent> AuditEvents { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseNpgsql(o => o
                    .UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery)
                    .UseNodaTime());
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasPostgresEnum<ElementCostType>();
            modelBuilder.HasPostgresEnum<ElementStatus>();
            modelBuilder.HasPostgresEnum<ProviderType>();
            modelBuilder.HasPostgresEnum<ReferralStatus>();
            modelBuilder.HasPostgresEnum<WorkflowType>();
            modelBuilder.HasPostgresEnum<UserRole>();
            modelBuilder.HasPostgresEnum<AuditEventType>();

            modelBuilder
                .Entity<CarePackage>()
                .ToView("care_packages")
                .HasKey(c => c.Id);

            modelBuilder.Entity<CarePackage>()
                .Property(c => c.Id)
                .ValueGeneratedNever();

            modelBuilder.Entity<CarePackage>()
                .HasMany(r => r.Elements)
                .WithMany(e => e.CarePackages)
                .UsingEntity<ReferralElement>(
                    j => j
                        .HasOne(re => re.Element)
                        .WithMany(e => e.ReferralElements)
                        .HasForeignKey(re => re.ElementId),
                    j => j
                        .HasOne(re => re.CarePackage)
                        .WithMany(e => e.ReferralElements)
                        .HasForeignKey(re => re.ReferralId),
                    j =>
                    {
                        j.HasKey(re => new { re.ElementId, re.ReferralId });
                    });

            modelBuilder.Entity<CarePackage>()
                .HasOne(cp => cp.AssignedBroker)
                .WithMany(b => b.BrokerCarePackages)
                .HasForeignKey("AssignedBrokerId");

            modelBuilder.Entity<CarePackage>()
                .HasOne(cp => cp.AssignedApprover)
                .WithMany(b => b.ApproverCarePackages)
                .HasForeignKey("AssignedApproverId");

            modelBuilder.Entity<Element>()
                .HasOne(e => e.ParentElement)
                .WithMany(e => e.ChildElements)
                .HasForeignKey("ParentElementId");

            modelBuilder.Entity<Element>()
                .Property(e => e.InternalStatus)
                .HasDefaultValue(ElementStatus.InProgress);

            modelBuilder.Entity<ElementType>()
                .Property(et => et.Id)
                .ValueGeneratedNever();

            modelBuilder.Entity<ElementType>()
                .Property(et => et.NonPersonalBudget)
                .HasDefaultValue(false);

            modelBuilder.Entity<ElementType>()
                .Property(et => et.IsArchived)
                .HasDefaultValue(false);

            modelBuilder.Entity<ElementType>()
                .HasIndex(et => new { et.ServiceId, et.Name })
                .IsUnique();

            modelBuilder.Entity<Provider>()
                .HasMany(p => p.Services)
                .WithMany(s => s.Providers)
                .UsingEntity<ProviderService>(
                    j => j
                        .HasOne(ps => ps.Service)
                        .WithMany(s => s.ProviderServices)
                        .HasForeignKey(ps => ps.ServiceId),
                    j => j
                        .HasOne(ps => ps.Provider)
                        .WithMany(p => p.ProviderServices)
                        .HasForeignKey(ps => ps.ProviderId),
                    j =>
                    {
                        j.HasKey(ps => new { ps.ProviderId, ps.ServiceId });
                    });

            modelBuilder.Entity<Provider>()
                .Property(p => p.IsArchived)
                .HasDefaultValue(false);

            modelBuilder.Entity<Provider>()
                .HasGeneratedTsVectorColumn(
                    p => p.SearchVector, "simple",
                    p => new { p.Name, p.Address })
                .HasIndex(p => p.SearchVector)
                .HasMethod("GIN");

            modelBuilder.Entity<Referral>()
                .HasIndex(r => r.WorkflowId)
                .IsUnique();

            modelBuilder.Entity<Referral>()
                .HasMany(r => r.Elements)
                .WithMany(e => e.Referrals)
                .UsingEntity<ReferralElement>(
                    j => j
                        .HasOne(re => re.Element)
                        .WithMany(e => e.ReferralElements)
                        .HasForeignKey(re => re.ElementId),
                    j => j
                        .HasOne(re => re.Referral)
                        .WithMany(e => e.ReferralElements)
                        .HasForeignKey(re => re.ReferralId),
                    j =>
                    {
                        j.HasKey(re => new { re.ElementId, re.ReferralId });
                    });

            modelBuilder.Entity<Service>()
                .Property(s => s.Id)
                .ValueGeneratedNever();

            modelBuilder.Entity<Service>()
                .Property(s => s.IsArchived)
                .HasDefaultValue(false);

            modelBuilder.Entity<User>()
                .HasPrincipalKey(ue => ue.Email);

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
            var currentTime = _clock.Now;

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
                            trackable.CreatedAt =
                                trackable.CreatedAt == NodaConstants.UnixEpoch ?
                                currentTime : trackable.CreatedAt;
                            trackable.UpdatedAt =
                                trackable.UpdatedAt == NodaConstants.UnixEpoch ?
                                currentTime : trackable.UpdatedAt;
                            break;
                    }
                }
            }
        }
    }
}
