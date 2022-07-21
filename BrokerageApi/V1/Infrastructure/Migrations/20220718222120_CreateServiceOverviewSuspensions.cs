using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace V1.Infrastructure.Migrations
{
    public partial class CreateServiceOverviewSuspensions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE VIEW service_overview_suspensions AS
                SELECT DISTINCT
                    e.id,
                    e.suspended_element_id,
                    FIRST_VALUE(r.id) OVER (
                        PARTITION BY re.element_id
                        ORDER BY r.updated_at DESC
                    ) AS referral_id,
                    e.start_date,
                    e.end_date,
                    e.internal_status,
                    e.quantity,
                    e.cost
                FROM
                    elements AS e
                INNER JOIN
                    referral_elements AS re ON e.id = re.element_id
                INNER JOIN
                    referrals AS r ON re.referral_id = r.id
                WHERE
                    e.internal_status = 'approved'
                AND
                    is_suspension = TRUE;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW service_overview_suspensions");
        }
    }
}
