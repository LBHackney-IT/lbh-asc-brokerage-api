﻿// <auto-generated />
using System;
using System.Collections.Generic;
using BrokerageApi.V1.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using NpgsqlTypes;

namespace V1.Infrastructure.Migrations
{
    [DbContext(typeof(BrokerageContext))]
    partial class BrokerageContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasPostgresEnum(null, "element_cost_type", new[] { "hourly", "daily", "weekly", "transport", "one_off" })
                .HasPostgresEnum(null, "provider_type", new[] { "framework", "spot" })
                .HasPostgresEnum(null, "referral_status", new[] { "unassigned", "in_review", "assigned", "on_hold", "archived", "in_progress", "awaiting_approval", "approved" })
                .HasPostgresEnum(null, "user_role", new[] { "brokerage_assistant", "broker", "approver", "care_charges_officer", "referrer" })
                .HasPostgresEnum(null, "workflow_type", new[] { "assessment", "review", "reassessment", "historic" })
                .HasAnnotation("Relational:MaxIdentifierLength", 63)
                .HasAnnotation("ProductVersion", "5.0.10")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            modelBuilder.Entity("BrokerageApi.V1.Infrastructure.ElementType", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    b.Property<ElementCostType>("CostType")
                        .HasColumnType("element_cost_type")
                        .HasColumnName("cost_type");

                    b.Property<bool>("IsArchived")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("boolean")
                        .HasDefaultValue(false)
                        .HasColumnName("is_archived");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.Property<bool>("NonPersonalBudget")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("boolean")
                        .HasDefaultValue(false)
                        .HasColumnName("non_personal_budget");

                    b.Property<int>("Position")
                        .HasColumnType("integer")
                        .HasColumnName("position");

                    b.Property<int>("ServiceId")
                        .HasColumnType("integer")
                        .HasColumnName("service_id");

                    b.HasKey("Id")
                        .HasName("pk_element_types");

                    b.HasIndex("ServiceId", "Name")
                        .IsUnique()
                        .HasDatabaseName("ix_element_types_service_id_name");

                    b.ToTable("element_types");
                });

            modelBuilder.Entity("BrokerageApi.V1.Infrastructure.Provider", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("address");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("created_at");

                    b.Property<bool>("IsArchived")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("boolean")
                        .HasDefaultValue(false)
                        .HasColumnName("is_archived");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.Property<NpgsqlTsVector>("SearchVector")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("tsvector")
                        .HasColumnName("search_vector")
                        .HasAnnotation("Npgsql:TsVectorConfig", "simple")
                        .HasAnnotation("Npgsql:TsVectorProperties", new[] { "Name", "Address" });

                    b.Property<ProviderType>("Type")
                        .HasColumnType("provider_type")
                        .HasColumnName("type");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("updated_at");

                    b.HasKey("Id")
                        .HasName("pk_providers");

                    b.HasIndex("SearchVector")
                        .HasDatabaseName("ix_providers_search_vector")
                        .HasMethod("GIN");

                    b.ToTable("providers");
                });

            modelBuilder.Entity("BrokerageApi.V1.Infrastructure.ProviderService", b =>
                {
                    b.Property<int>("ProviderId")
                        .HasColumnType("integer")
                        .HasColumnName("provider_id");

                    b.Property<int>("ServiceId")
                        .HasColumnType("integer")
                        .HasColumnName("service_id");

                    b.Property<string>("SubjectiveCode")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("subjective_code");

                    b.HasKey("ProviderId", "ServiceId")
                        .HasName("pk_provider_services");

                    b.HasIndex("ServiceId")
                        .HasDatabaseName("ix_provider_services_service_id");

                    b.ToTable("provider_services");
                });

            modelBuilder.Entity("BrokerageApi.V1.Infrastructure.Referral", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("AssignedTo")
                        .HasColumnType("text")
                        .HasColumnName("assigned_to");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("created_at");

                    b.Property<string>("FormName")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("form_name");

                    b.Property<string>("Note")
                        .HasColumnType("text")
                        .HasColumnName("note");

                    b.Property<string>("PrimarySupportReason")
                        .HasColumnType("text")
                        .HasColumnName("primary_support_reason");

                    b.Property<string>("ResidentName")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("resident_name");

                    b.Property<string>("SocialCareId")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("social_care_id");

                    b.Property<ReferralStatus>("Status")
                        .HasColumnType("referral_status")
                        .HasColumnName("status");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("updated_at");

                    b.Property<DateTime?>("UrgentSince")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("urgent_since");

                    b.Property<string>("WorkflowId")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("workflow_id");

                    b.Property<WorkflowType>("WorkflowType")
                        .HasColumnType("workflow_type")
                        .HasColumnName("workflow_type");

                    b.HasKey("Id")
                        .HasName("pk_referrals");

                    b.HasIndex("WorkflowId")
                        .IsUnique()
                        .HasDatabaseName("ix_referrals_workflow_id");

                    b.ToTable("referrals");
                });

            modelBuilder.Entity("BrokerageApi.V1.Infrastructure.Service", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    b.Property<string>("Description")
                        .HasColumnType("text")
                        .HasColumnName("description");

                    b.Property<bool>("IsArchived")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("boolean")
                        .HasDefaultValue(false)
                        .HasColumnName("is_archived");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.Property<int?>("ParentId")
                        .HasColumnType("integer")
                        .HasColumnName("parent_id");

                    b.Property<int>("Position")
                        .HasColumnType("integer")
                        .HasColumnName("position");

                    b.HasKey("Id")
                        .HasName("pk_services");

                    b.HasIndex("ParentId")
                        .HasDatabaseName("ix_services_parent_id");

                    b.ToTable("services");
                });

            modelBuilder.Entity("BrokerageApi.V1.Infrastructure.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("created_at");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("email");

                    b.Property<bool>("IsActive")
                        .HasColumnType("boolean")
                        .HasColumnName("is_active");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.Property<List<UserRole>>("Roles")
                        .HasColumnType("user_role[]")
                        .HasColumnName("roles");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("updated_at");

                    b.HasKey("Id")
                        .HasName("pk_users");

                    b.HasIndex("Email")
                        .IsUnique()
                        .HasDatabaseName("ix_users_email");

                    b.ToTable("users");
                });

            modelBuilder.Entity("BrokerageApi.V1.Infrastructure.ElementType", b =>
                {
                    b.HasOne("BrokerageApi.V1.Infrastructure.Service", "Service")
                        .WithMany("ElementTypes")
                        .HasForeignKey("ServiceId")
                        .HasConstraintName("fk_element_types_services_service_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Service");
                });

            modelBuilder.Entity("BrokerageApi.V1.Infrastructure.ProviderService", b =>
                {
                    b.HasOne("BrokerageApi.V1.Infrastructure.Provider", "Provider")
                        .WithMany("ProviderServices")
                        .HasForeignKey("ProviderId")
                        .HasConstraintName("fk_provider_services_providers_provider_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("BrokerageApi.V1.Infrastructure.Service", "Service")
                        .WithMany("ProviderServices")
                        .HasForeignKey("ServiceId")
                        .HasConstraintName("fk_provider_services_services_service_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Provider");

                    b.Navigation("Service");
                });

            modelBuilder.Entity("BrokerageApi.V1.Infrastructure.Service", b =>
                {
                    b.HasOne("BrokerageApi.V1.Infrastructure.Service", "Parent")
                        .WithMany("Services")
                        .HasForeignKey("ParentId")
                        .HasConstraintName("fk_services_services_parent_id");

                    b.Navigation("Parent");
                });

            modelBuilder.Entity("BrokerageApi.V1.Infrastructure.Provider", b =>
                {
                    b.Navigation("ProviderServices");
                });

            modelBuilder.Entity("BrokerageApi.V1.Infrastructure.Service", b =>
                {
                    b.Navigation("ElementTypes");

                    b.Navigation("ProviderServices");

                    b.Navigation("Services");
                });
#pragma warning restore 612, 618
        }
    }
}
