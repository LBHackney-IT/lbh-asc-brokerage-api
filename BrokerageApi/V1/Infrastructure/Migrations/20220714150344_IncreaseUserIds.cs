using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace V1.Infrastructure.Migrations
{
    public partial class IncreaseUserIds : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE audit_events
                DROP CONSTRAINT fk_audit_events_users_user_id
            ");

            migrationBuilder.Sql(@"
                UPDATE users SET id = id + 1000
            ");

            migrationBuilder.Sql(@"
                UPDATE audit_events SET user_id = user_id + 1000
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE audit_events
                ADD CONSTRAINT fk_audit_events_users_user_id
                FOREIGN KEY (user_id) REFERENCES users(id)
            ");

            migrationBuilder.Sql(@"
                SELECT setval('users_id_seq', (SELECT COALESCE(MAX(id), 1) FROM users))
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE audit_events
                DROP CONSTRAINT fk_audit_events_users_user_id
            ");

            migrationBuilder.Sql(@"
                UPDATE users SET id = id - 1000
            ");

            migrationBuilder.Sql(@"
                UPDATE audit_events SET user_id = user_id - 1000
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE audit_events
                ADD CONSTRAINT fk_audit_events_users_user_id
                FOREIGN KEY (user_id) REFERENCES users(id)
            ");

            migrationBuilder.Sql(@"
                SELECT setval('users_id_seq', (SELECT COALESCE(MAX(id), 1) FROM users))
            ");
        }
    }
}
