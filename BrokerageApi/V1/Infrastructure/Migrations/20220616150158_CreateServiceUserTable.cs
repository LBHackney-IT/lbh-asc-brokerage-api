using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

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
                    mosaic_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    service_user_name = table.Column<string>(type: "text", nullable: false),
                    date_of_birth = table.Column<Instant>(type: "timestamp", nullable: false),
                    created_at = table.Column<Instant>(type: "timestamp", nullable: false),
                    updated_at = table.Column<Instant>(type: "timestamp", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_service_users", x => x.mosaic_id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "service_users");
        }
    }
}
