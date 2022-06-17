using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

namespace V1.Infrastructure.Migrations
{
    public partial class CreateServiceUserTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "service_users",
                columns: table => new
                {
                    social_care_id = table.Column<string>(type: "text", nullable: false),
                    service_user_name = table.Column<string>(type: "text", nullable: false),
                    date_of_birth = table.Column<LocalDate>(type: "date", nullable: false),
                    created_at = table.Column<Instant>(type: "timestamp", nullable: false),
                    updated_at = table.Column<Instant>(type: "timestamp", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_service_users", x => x.social_care_id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "service_users");
        }
    }
}
