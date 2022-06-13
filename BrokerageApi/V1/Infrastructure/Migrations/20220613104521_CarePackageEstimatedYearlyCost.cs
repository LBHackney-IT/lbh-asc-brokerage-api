using Microsoft.EntityFrameworkCore.Migrations;

namespace V1.Infrastructure.Migrations
{
    public partial class CarePackageEstimatedYearlyCost : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
		            n.care_package_name,
                    r.assigned_broker_email AS assigned_broker_id,
                    r.assigned_approver_email AS assigned_approver_id,
                    r.status,
                    r.note,
                    r.started_at,
                    r.created_at,
                    r.updated_at,
                    d.start_date,
                    c.weekly_cost,
                    p.weekly_payment,
                    o.one_off_payment,
                    r.comment,
                    (COALESCE(p.weekly_payment, 0) * 52) + COALESCE(o.one_off_payment, 0) AS estimated_yearly_cost
                FROM
                    referrals AS r
                LEFT JOIN
                    care_package_start_dates AS d ON r.id = d.referral_id
                LEFT JOIN
                    care_package_weekly_costs AS c ON r.id = c.referral_id
                LEFT JOIN
                    care_package_weekly_payments AS p ON r.id = p.referral_id
                LEFT JOIN
                    care_package_name AS n ON r.id = n.referral_id
                LEFT JOIN
                    care_package_one_off_payment AS o ON r.id = o.referral_id
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
		            n.care_package_name,
                    r.assigned_broker_email AS assigned_broker_id,
                    r.assigned_approver_email AS assigned_approver_id,
                    r.status,
                    r.note,
                    r.started_at,
                    r.created_at,
                    r.updated_at,
                    d.start_date,
                    c.weekly_cost,
                    p.weekly_payment,
                    o.one_off_payment,
                    r.comment
                FROM
                    referrals AS r
                LEFT JOIN
                    care_package_start_dates AS d ON r.id = d.referral_id
                LEFT JOIN
                    care_package_weekly_costs AS c ON r.id = c.referral_id
                LEFT JOIN
                    care_package_weekly_payments AS p ON r.id = p.referral_id
                LEFT JOIN
                    care_package_name AS n ON r.id = n.referral_id
                LEFT JOIN
                    care_package_one_off_payment AS o ON r.id = o.referral_id
            ");
        }
    }
}
