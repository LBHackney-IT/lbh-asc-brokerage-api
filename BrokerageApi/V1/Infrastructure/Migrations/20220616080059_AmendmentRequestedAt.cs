using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

namespace V1.Infrastructure.Migrations
{
    public partial class AmendmentRequestedAt : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Instant>(
                name: "requested_at",
                table: "referral_amendment",
                type: "timestamp",
                nullable: false,
                defaultValue: NodaTime.Instant.FromUnixTimeTicks(0L));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "requested_at",
                table: "referral_amendment");
        }
    }
}
