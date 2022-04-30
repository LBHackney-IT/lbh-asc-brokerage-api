using BrokerageApi.V1.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace V1.Infrastructure.Migrations
{
    public partial class CreateElement : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:element_cost_type", "hourly,daily,weekly,transport,one_off")
                .Annotation("Npgsql:Enum:element_status", "in_progress,awaiting_approval,approved,inactive,active,ended,suspended")
                .Annotation("Npgsql:Enum:provider_type", "framework,spot")
                .Annotation("Npgsql:Enum:referral_status", "unassigned,in_review,assigned,on_hold,archived,in_progress,awaiting_approval,approved")
                .Annotation("Npgsql:Enum:user_role", "brokerage_assistant,broker,approver,care_charges_officer,referrer")
                .Annotation("Npgsql:Enum:workflow_type", "assessment,review,reassessment,historic")
                .OldAnnotation("Npgsql:Enum:element_cost_type", "hourly,daily,weekly,transport,one_off")
                .OldAnnotation("Npgsql:Enum:provider_type", "framework,spot")
                .OldAnnotation("Npgsql:Enum:referral_status", "unassigned,in_review,assigned,on_hold,archived,in_progress,awaiting_approval,approved")
                .OldAnnotation("Npgsql:Enum:user_role", "brokerage_assistant,broker,approver,care_charges_officer,referrer")
                .OldAnnotation("Npgsql:Enum:workflow_type", "assessment,review,reassessment,historic");

            migrationBuilder.CreateTable(
                name: "elements",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    social_care_id = table.Column<string>(type: "text", nullable: false),
                    element_type_id = table.Column<int>(type: "integer", nullable: false),
                    non_personal_budget = table.Column<bool>(type: "boolean", nullable: false),
                    provider_id = table.Column<int>(type: "integer", nullable: false),
                    details = table.Column<string>(type: "text", nullable: false),
                    internal_status = table.Column<ElementStatus>(type: "element_status", nullable: false, defaultValue: ElementStatus.InProgress),
                    related_element_id = table.Column<int>(type: "integer", nullable: true),
                    start_date = table.Column<LocalDate>(type: "date", nullable: false),
                    end_date = table.Column<LocalDate>(type: "date", nullable: true),
                    monday = table.Column<ElementCost>(type: "jsonb", nullable: true),
                    tuesday = table.Column<ElementCost>(type: "jsonb", nullable: true),
                    wednesday = table.Column<ElementCost>(type: "jsonb", nullable: true),
                    thursday = table.Column<ElementCost>(type: "jsonb", nullable: true),
                    friday = table.Column<ElementCost>(type: "jsonb", nullable: true),
                    saturday = table.Column<ElementCost>(type: "jsonb", nullable: true),
                    sunday = table.Column<ElementCost>(type: "jsonb", nullable: true),
                    quantity = table.Column<decimal>(type: "numeric", nullable: true),
                    cost = table.Column<decimal>(type: "numeric", nullable: false),
                    created_at = table.Column<Instant>(type: "timestamp", nullable: false),
                    updated_at = table.Column<Instant>(type: "timestamp", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_elements", x => x.id);
                    table.ForeignKey(
                        name: "fk_elements_element_types_element_type_id",
                        column: x => x.element_type_id,
                        principalTable: "element_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_elements_elements_related_element_id",
                        column: x => x.related_element_id,
                        principalTable: "elements",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_elements_providers_provider_id",
                        column: x => x.provider_id,
                        principalTable: "providers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "referral_elements",
                columns: table => new
                {
                    referral_id = table.Column<int>(type: "integer", nullable: false),
                    element_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_referral_elements", x => new { x.element_id, x.referral_id });
                    table.ForeignKey(
                        name: "fk_referral_elements_elements_element_id",
                        column: x => x.element_id,
                        principalTable: "elements",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_referral_elements_referrals_referral_id",
                        column: x => x.referral_id,
                        principalTable: "referrals",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_elements_element_type_id",
                table: "elements",
                column: "element_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_elements_provider_id",
                table: "elements",
                column: "provider_id");

            migrationBuilder.CreateIndex(
                name: "ix_elements_related_element_id",
                table: "elements",
                column: "related_element_id");

            migrationBuilder.CreateIndex(
                name: "ix_referral_elements_referral_id",
                table: "referral_elements",
                column: "referral_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "referral_elements");

            migrationBuilder.DropTable(
                name: "elements");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:element_cost_type", "hourly,daily,weekly,transport,one_off")
                .Annotation("Npgsql:Enum:provider_type", "framework,spot")
                .Annotation("Npgsql:Enum:referral_status", "unassigned,in_review,assigned,on_hold,archived,in_progress,awaiting_approval,approved")
                .Annotation("Npgsql:Enum:user_role", "brokerage_assistant,broker,approver,care_charges_officer,referrer")
                .Annotation("Npgsql:Enum:workflow_type", "assessment,review,reassessment,historic")
                .OldAnnotation("Npgsql:Enum:element_cost_type", "hourly,daily,weekly,transport,one_off")
                .OldAnnotation("Npgsql:Enum:element_status", "in_progress,awaiting_approval,approved,inactive,active,ended,suspended")
                .OldAnnotation("Npgsql:Enum:provider_type", "framework,spot")
                .OldAnnotation("Npgsql:Enum:referral_status", "unassigned,in_review,assigned,on_hold,archived,in_progress,awaiting_approval,approved")
                .OldAnnotation("Npgsql:Enum:user_role", "brokerage_assistant,broker,approver,care_charges_officer,referrer")
                .OldAnnotation("Npgsql:Enum:workflow_type", "assessment,review,reassessment,historic");
        }
    }
}
