using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace V1.Infrastructure.Migrations
{
    public partial class CreateServiceOverviewElements : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE VIEW service_overview_elements AS
                SELECT DISTINCT
                    e.id,
                    e.social_care_id,
                    FIRST_VALUE(r.id) OVER (
                        PARTITION BY re.element_id
                        ORDER BY r.updated_at DESC
                    ) AS referral_id,
                    et.service_id,
                    et.type,
                    et.name,
                    e.provider_id,
                    e.start_date,
                    e.end_date,
                    e.internal_status,
                    et.payment_cycle,
                    e.quantity,
                    e.cost
                FROM
                    elements AS e
                INNER JOIN
                    element_types AS et ON e.element_type_id = et.id
                INNER JOIN
                    services AS s ON et.service_id = s.id
                INNER JOIN
                    referral_elements AS re ON e.id = re.element_id
                INNER JOIN
                    referrals AS r ON re.referral_id = r.id
                WHERE
                    e.internal_status = 'approved'
                AND
                    is_suspension = FALSE
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW service_overview_elements");
        }
    }
}
