using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace V1.Infrastructure.Migrations
{
    public partial class AddCareChargesConfirmedAtToReferral : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Instant>(
                name: "care_charges_confirmed_at",
                table: "referrals",
                type: "timestamp with time zone",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "care_charges_confirmed_at",
                table: "referrals");
        }
    }
}
