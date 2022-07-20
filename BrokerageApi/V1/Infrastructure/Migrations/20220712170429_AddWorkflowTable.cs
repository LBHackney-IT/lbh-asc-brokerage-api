using BrokerageApi.V1.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace V1.Infrastructure.Migrations
{
    public partial class AddWorkflowTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "workflows",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    referral_id = table.Column<int>(type: "integer", nullable: false),
                    form_name = table.Column<string>(type: "text", nullable: false),
                    workflow_type = table.Column<WorkflowType>(type: "workflow_type", nullable: false),
                    note = table.Column<string>(type: "text", nullable: true),
                    primary_support_reason = table.Column<string>(type: "text", nullable: true),
                    direct_payments = table.Column<string>(type: "text", nullable: true),
                    urgent_since = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_workflows", x => x.id);
                    table.ForeignKey(
                        name: "fk_workflows_referrals_referral_id",
                        column: x => x.referral_id,
                        principalTable: "referrals",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_workflows_referral_id",
                table: "workflows",
                column: "referral_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "workflows");
        }
    }
}
