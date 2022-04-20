using BrokerageApi.V1.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace V1.Infrastructure.Migrations
{
    public partial class CreateElementType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:element_cost_type", "hourly,daily,weekly,transport,one_off")
                .Annotation("Npgsql:Enum:referral_status", "unassigned,in_review,assigned,on_hold,archived,in_progress,awaiting_approval,approved")
                .Annotation("Npgsql:Enum:user_role", "brokerage_assistant,broker,approver,care_charges_officer,referrer")
                .Annotation("Npgsql:Enum:workflow_type", "assessment,review,reassessment,historic")
                .OldAnnotation("Npgsql:Enum:referral_status", "unassigned,in_review,assigned,on_hold,archived,in_progress,awaiting_approval,approved")
                .OldAnnotation("Npgsql:Enum:user_role", "brokerage_assistant,broker,approver,care_charges_officer,referrer")
                .OldAnnotation("Npgsql:Enum:workflow_type", "assessment,review,reassessment,historic");

            migrationBuilder.CreateTable(
                name: "element_types",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    service_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    cost_type = table.Column<ElementCostType>(type: "element_cost_type", nullable: false),
                    non_personal_budget = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    position = table.Column<int>(type: "integer", nullable: false),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_element_types", x => x.id);
                    table.ForeignKey(
                        name: "fk_element_types_services_service_id",
                        column: x => x.service_id,
                        principalTable: "services",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_element_types_service_id_name",
                table: "element_types",
                columns: new[] { "service_id", "name" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "element_types");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:referral_status", "unassigned,in_review,assigned,on_hold,archived,in_progress,awaiting_approval,approved")
                .Annotation("Npgsql:Enum:user_role", "brokerage_assistant,broker,approver,care_charges_officer,referrer")
                .Annotation("Npgsql:Enum:workflow_type", "assessment,review,reassessment,historic")
                .OldAnnotation("Npgsql:Enum:element_cost_type", "hourly,daily,weekly,transport,one_off")
                .OldAnnotation("Npgsql:Enum:referral_status", "unassigned,in_review,assigned,on_hold,archived,in_progress,awaiting_approval,approved")
                .OldAnnotation("Npgsql:Enum:user_role", "brokerage_assistant,broker,approver,care_charges_officer,referrer")
                .OldAnnotation("Npgsql:Enum:workflow_type", "assessment,review,reassessment,historic");
        }
    }
}
