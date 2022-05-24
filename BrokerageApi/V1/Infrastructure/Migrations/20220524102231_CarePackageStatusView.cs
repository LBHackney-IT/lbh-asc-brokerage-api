using Microsoft.EntityFrameworkCore.Migrations;

namespace V1.Infrastructure.Migrations
{
    public partial class CarePackageStatusView : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE VIEW care_package_status AS
                SELECT r.id AS referral_id,
                    CASE
                        WHEN r.status = 'approved' AND COUNT(active_elements) > 0 THEN 'active'
                        WHEN r.status = 'approved' AND COUNT(active_elements) <= 0 AND COUNT(future_elements) <= 0 THEN 'ended'
                        ELSE r.status
                    END AS referral_status
                FROM referrals r
                    LEFT JOIN referral_elements re ON r.id = re.referral_id
                    LEFT JOIN elements active_elements ON re.element_id = active_elements.id AND active_elements.internal_status = 'approved' AND active_elements.start_date <= CURRENT_DATE AND (active_elements.end_date IS NULL OR active_elements.end_date >= CURRENT_DATE)
                    LEFT JOIN elements future_elements ON re.element_id = future_elements.id AND future_elements.internal_status = 'approved' AND future_elements.start_date > CURRENT_DATE
                GROUP BY r.id
            ");

            migrationBuilder.Sql("DROP VIEW care_packages");

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
                    s.referral_status,
                    r.note,
                    r.started_at,
                    r.created_at,
                    r.updated_at,
                    d.start_date,
                    c.weekly_cost,
                    p.weekly_payment
                FROM
                    referrals AS r
                JOIN
                    care_package_status AS s ON s.referral_id = r.id
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
            migrationBuilder.Sql("DROP VIEW care_package_status");
            migrationBuilder.Sql("DROP VIEW care_packages");

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
