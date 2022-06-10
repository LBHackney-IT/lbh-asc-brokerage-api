using Microsoft.EntityFrameworkCore.Migrations;

namespace V1.Infrastructure.Migrations
{
    public partial class ConvertAuditEventMetadataToJsonb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE audit_events ALTER COLUMN metadata TYPE jsonb USING metadata::jsonb");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE audit_events ALTER COLUMN metadata TYPE text USING metadata::text");
        }
    }
}
