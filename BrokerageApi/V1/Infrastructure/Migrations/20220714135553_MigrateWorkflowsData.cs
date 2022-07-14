using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace V1.Infrastructure.Migrations
{
    public partial class MigrateWorkflowsData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                INSERT INTO workflows (id, referral_id, form_name, workflow_type, note, primary_support_reason, direct_payments, urgent_since, created_at, updated_at)
                SELECT workflow_id, id, form_name, workflow_type, note, primary_support_reason, direct_payments, urgent_since, created_at, updated_at
                FROM referrals
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("TRUNCATE table workflows");
        }
    }
}
