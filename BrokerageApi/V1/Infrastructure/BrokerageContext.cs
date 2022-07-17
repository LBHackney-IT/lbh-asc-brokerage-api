using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Npgsql;
using BrokerageApi.V1.Infrastructure.AuditEvents;
using BrokerageApi.V1.Services.Interfaces;

namespace BrokerageApi.V1.Infrastructure
{
    public class BrokerageContext : DbContext
    {
        private readonly IClockService _clock;

        static BrokerageContext()
        {
            NpgsqlConnection.GlobalTypeMapper.UseNodaTime();

            NpgsqlConnection.GlobalTypeMapper.MapEnum<CareChargeStatus>("care_charge_status");
            NpgsqlConnection.GlobalTypeMapper.MapEnum<ElementBillingType>("element_billing_type");
            NpgsqlConnection.GlobalTypeMapper.MapEnum<ElementCostType>("element_cost_type");
            NpgsqlConnection.GlobalTypeMapper.MapEnum<ElementStatus>("element_status");
            NpgsqlConnection.GlobalTypeMapper.MapEnum<ElementTypeType>("element_type_type");
            NpgsqlConnection.GlobalTypeMapper.MapEnum<ProviderType>("provider_type");
            NpgsqlConnection.GlobalTypeMapper.MapEnum<ReferralStatus>("referral_status");
            NpgsqlConnection.GlobalTypeMapper.MapEnum<WorkflowType>("workflow_type");
            NpgsqlConnection.GlobalTypeMapper.MapEnum<UserRole>("user_role");
            NpgsqlConnection.GlobalTypeMapper.MapEnum<AuditEventType>("audit_event_type");
            NpgsqlConnection.GlobalTypeMapper.MapEnum<AmendmentStatus>("amendment_status");
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
        public DbSet<Referral> Referrals { get; set; }
        public DbSet<ReferralElement> ReferralElements { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<AuditEvent> AuditEvents { get; set; }
        public DbSet<ServiceUser> ServiceUsers { get; set; }
        public DbSet<Workflow> Workflows { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseNpgsql(o => o
                    .UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery)
                    .UseNodaTime());
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasPostgresEnum<CareChargeStatus>();
            modelBuilder.HasPostgresEnum<ElementBillingType>();
            modelBuilder.HasPostgresEnum<ElementCostType>();
            modelBuilder.HasPostgresEnum<ElementStatus>();
            modelBuilder.HasPostgresEnum<ElementTypeType>();
            modelBuilder.HasPostgresEnum<ProviderType>();
            modelBuilder.HasPostgresEnum<ReferralStatus>();
            modelBuilder.HasPostgresEnum<WorkflowType>();
            modelBuilder.HasPostgresEnum<UserRole>();
            modelBuilder.HasPostgresEnum<AuditEventType>();
            modelBuilder.HasPostgresEnum<AmendmentStatus>();

            modelBuilder.Entity<AuditEvent>()
                .Property(ae => ae.Metadata)
                .HasColumnType("jsonb");

            modelBuilder.Entity<AuditEvent>()
                .Property(ae => ae.ReferralId)
                .HasComputedColumnSql(@"(metadata->>'referralId')::integer", stored: true);

            modelBuilder.Entity<AuditEvent>()
                .HasIndex(ae => ae.ReferralId);

            modelBuilder.Entity<AuditEvent>()
                .HasOne(ae => ae.Referral)
                .WithMany()
                .HasForeignKey("ReferralId");

            modelBuilder.Entity<CarePackage>()
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
                .HasMany(c => c.ReferralAmendments)
                .WithOne()
                .HasForeignKey("ReferralId");

            modelBuilder.Entity<ReferralAmendment>()
                .HasOne(a => a.Referral)
                .WithMany(r => r.ReferralAmendments)
                .HasForeignKey("ReferralId");

            modelBuilder.Entity<CarePackage>()
                .HasOne(cp => cp.AssignedBroker)
                .WithMany(u => u.BrokerCarePackages)
                .HasForeignKey("AssignedBrokerId")
                .HasPrincipalKey("Email");

            modelBuilder.Entity<CarePackage>()
                .HasOne(cp => cp.AssignedApprover)
                .WithMany(u => u.ApproverCarePackages)
                .HasForeignKey("AssignedApproverId")
                .HasPrincipalKey("Email");

            modelBuilder.Entity<CarePackage>()
              .HasMany(cp => cp.Workflows)
              .WithOne()
              .HasForeignKey("ReferralId");

            modelBuilder.Entity<Workflow>()
              .HasOne(a => a.Referral)
              .WithMany(r => r.Workflows)
              .HasForeignKey("ReferralId");

            modelBuilder.Entity<Referral>()
                .HasOne(cp => cp.AssignedBroker)
                .WithMany()
                .HasForeignKey("AssignedBrokerEmail")
                .HasPrincipalKey("Email");

            modelBuilder.Entity<Referral>()
                .HasOne(cp => cp.AssignedApprover)
                .WithMany()
                .HasForeignKey("AssignedApproverEmail")
                .HasPrincipalKey("Email");

            modelBuilder.Entity<Element>()
                .HasOne(e => e.ParentElement)
                .WithMany(e => e.ChildElements)
                .HasForeignKey("ParentElementId");

            modelBuilder.Entity<Element>()
                .HasOne(e => e.SuspendedElement)
                .WithMany(e => e.SuspensionElements)
                .HasForeignKey("SuspendedElementId");

            modelBuilder.Entity<Element>()
                .Property(e => e.IsSuspension)
                .HasDefaultValue(false);

            modelBuilder.Entity<Element>()
                .Property(e => e.InternalStatus)
                .HasDefaultValue(ElementStatus.InProgress);

            modelBuilder.Entity<Element>()
                .Property(e => e.DailyCosts)
                .HasComputedColumnSql(@"ARRAY[COALESCE((monday->>'Cost')::numeric, 0), COALESCE((tuesday->>'Cost')::numeric, 0), COALESCE((wednesday->>'Cost')::numeric, 0), COALESCE((thursday->>'Cost')::numeric, 0), COALESCE((friday->>'Cost')::numeric, 0), COALESCE((saturday->>'Cost')::numeric, 0), COALESCE((sunday->>'Cost')::numeric, 0)]", stored: true);

            modelBuilder.Entity<ElementType>()
                .Property(et => et.Id)
                .ValueGeneratedNever();

            modelBuilder.Entity<ElementType>()
                .Property(et => et.Billing)
                .HasDefaultValue(ElementBillingType.Supplier);

            modelBuilder.Entity<ElementType>()
                .Property(et => et.CostOperation)
                .HasDefaultValue(MathOperation.Ignore);

            modelBuilder.Entity<ElementType>()
                .Property(et => et.PaymentOperation)
                .HasDefaultValue(MathOperation.Ignore);

            modelBuilder.Entity<ElementType>()
                .Property(et => et.NonPersonalBudget)
                .HasDefaultValue(false);

            modelBuilder.Entity<ElementType>()
                .Property(et => et.IsS117)
                .HasDefaultValue(false);

            modelBuilder.Entity<ElementType>()
                .Property(et => et.IsArchived)
                .HasDefaultValue(false);

            modelBuilder.Entity<ElementType>()
                .Property(et => et.IsResidential)
                .HasDefaultValue(false);

            modelBuilder.Entity<ElementType>()
                .Property(et => et.Type)
                .HasDefaultValue(ElementTypeType.Service);

            modelBuilder.Entity<ElementType>()
                .HasIndex(et => new { et.ServiceId, et.Name })
                .IsUnique();

            modelBuilder.Entity<Provider>()
                .HasIndex(p => new { p.CedarNumber, p.CedarSite })
                .IsUnique();

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
                .Property(r => r.CareChargeStatus)
                .HasDefaultValue(CareChargeStatus.New);

            modelBuilder.Entity<Referral>()
                .HasIndex(r => r.WorkflowId)
                .IsUnique();

            modelBuilder.Entity<Referral>()
                .HasIndex(r => r.SocialCareId)
                .HasFilter("status = 'in_progress' OR status = 'awaiting_approval'")
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

            modelBuilder.Entity<ReferralElement>()
                .Property(re => re.PendingCancellation)
                .HasDefaultValue(false);

            modelBuilder.Entity<Service>()
                .Property(s => s.Id)
                .ValueGeneratedNever();

            modelBuilder.Entity<Service>()
                .Property(s => s.IsArchived)
                .HasDefaultValue(false);

            modelBuilder.Entity<Service>()
                .Property(s => s.HasProvisionalClientContributions)
                .HasDefaultValue(false);

            modelBuilder.Entity<ServiceUser>()
                .HasGeneratedTsVectorColumn(
                    s => s.NameSearchVector, "simple",
                    s => new { s.ServiceUserName });

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
