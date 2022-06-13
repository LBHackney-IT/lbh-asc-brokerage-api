using Microsoft.EntityFrameworkCore.Migrations;

namespace V1.Infrastructure.Migrations
{
    public partial class AddReferralIdToAuditEvent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "referral_id",
                table: "audit_events",
                type: "integer",
                nullable: true,
                computedColumnSql: "(metadata->>'referralId')::integer",
                stored: true);

            migrationBuilder.CreateIndex(
                name: "ix_audit_events_referral_id",
                table: "audit_events",
                column: "referral_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_audit_events_referral_id",
                table: "audit_events");

            migrationBuilder.DropColumn(
                name: "referral_id",
                table: "audit_events");
        }
    }
}
