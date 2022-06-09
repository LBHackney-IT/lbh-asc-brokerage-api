using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

namespace V1.Infrastructure.Migrations
{
    public partial class AddReferralElementsPendingColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "pending_cancellation",
                table: "referral_elements",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "pending_comment",
                table: "referral_elements",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<LocalDate>(
                name: "pending_end_date",
                table: "referral_elements",
                type: "date",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "pending_cancellation",
                table: "referral_elements");

            migrationBuilder.DropColumn(
                name: "pending_comment",
                table: "referral_elements");

            migrationBuilder.DropColumn(
                name: "pending_end_date",
                table: "referral_elements");
        }
    }
}
