using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace V1.Infrastructure.Migrations
{
    public partial class CreateSystemUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                INSERT INTO users (id, name, email, is_active, created_at, updated_at)
                VALUES (1, 'system', 'system@localhost', FALSE, now(), now())
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DELETE FROM users WHERE name = 'system'
            ");
        }
    }
}
