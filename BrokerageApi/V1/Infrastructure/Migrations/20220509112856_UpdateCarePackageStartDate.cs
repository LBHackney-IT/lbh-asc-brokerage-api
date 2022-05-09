using Microsoft.EntityFrameworkCore.Migrations;

namespace V1.Infrastructure.Migrations
{
    public partial class UpdateCarePackageStartDate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW care_packages");
            migrationBuilder.Sql("DROP VIEW care_package_start_dates");

            migrationBuilder.Sql(@"
                CREATE VIEW care_package_start_dates AS
                SELECT
                    r.id AS referral_id,
                    MIN(e.start_date) AS start_date
                FROM
                    referrals AS r
                INNER JOIN
                    referral_elements AS re ON r.id = re.referral_id
                INNER JOIN
                    elements AS e ON re.element_id = e.id
                GROUP BY
                    r.id
            ");

            migrationBuilder.Sql(@"
                CREATE VIEW care_packages AS
                SELECT
                    r.id,
                    r.workflow_id,
                    r.workflow_type,
                    r.form_name,
                    r.social_care_id,
                    r.resident_name,
                    r.primary_support_reason,
                    r.urgent_since,
                    r.assigned_to,
                    r.status,
                    r.note,
                    r.started_at,
                    r.created_at,
                    r.updated_at,
                    d.start_date,
                    c.weekly_cost,
                    p.weekly_payment
                FROM
                    referrals AS r
                LEFT JOIN
                    care_package_start_dates AS d ON r.id = d.referral_id
                LEFT JOIN
                    care_package_weekly_costs AS c ON r.id = c.referral_id
                LEFT JOIN
                    care_package_weekly_payments AS p ON r.id = p.referral_id
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW care_packages");
            migrationBuilder.Sql("DROP VIEW care_package_start_dates");

            migrationBuilder.Sql(@"
                CREATE VIEW care_package_start_dates AS
                SELECT
                    r.id AS referral_id,
                    MIN(e.start_date) AS start_date
                FROM
                    referrals AS r
                INNER JOIN
                    referral_elements AS re ON r.id = re.referral_id
                INNER JOIN
                    elements AS e ON re.element_id = e.id
                WHERE
                    e.created_at > r.created_at
                GROUP BY
                    r.id
            ");

            migrationBuilder.Sql(@"
                CREATE VIEW care_packages AS
                SELECT
                    r.id,
                    r.workflow_id,
                    r.workflow_type,
                    r.form_name,
                    r.social_care_id,
                    r.resident_name,
                    r.primary_support_reason,
                    r.urgent_since,
                    r.assigned_to,
                    r.status,
                    r.note,
                    r.started_at,
                    r.created_at,
                    r.updated_at,
                    d.start_date,
                    c.weekly_cost,
                    p.weekly_payment
                FROM
                    referrals AS r
                LEFT JOIN
                    care_package_start_dates AS d ON r.id = d.referral_id
                LEFT JOIN
                    care_package_weekly_costs AS c ON r.id = c.referral_id
                LEFT JOIN
                    care_package_weekly_payments AS p ON r.id = p.referral_id
            ");
        }
    }
}
