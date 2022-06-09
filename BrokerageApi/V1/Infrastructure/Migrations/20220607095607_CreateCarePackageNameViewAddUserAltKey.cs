using Microsoft.EntityFrameworkCore.Migrations;

namespace V1.Infrastructure.Migrations
{
    public partial class CreateCarePackageNameViewAddUserAltKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddUniqueConstraint(
            name: "ak_users_email",
            table: "users",
            column: "email");

            migrationBuilder.Sql(@"
                CREATE VIEW care_package_name AS
                    SELECT r.id AS referral_id,
                            string_agg(DISTINCT s.name, ', '::text) AS care_package_name
                    FROM referrals r
                            JOIN referral_elements re ON r.id = re.referral_id
                            JOIN elements e ON re.element_id = e.id
                            JOIN element_types et ON e.element_type_id = et.id
                            JOIN services s ON et.service_id = s.id
                    GROUP BY r.id;
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
		            n.care_package_name,
                    r.assigned_broker AS assigned_broker_id,
                    r.assigned_approver AS assigned_approver_id,
                    r.status,
                    r.note,
                    r.started_at,
                    r.created_at,
                    r.updated_at,
                    d.start_date,
                    c.weekly_cost,
                    p.weekly_payment,
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
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "ak_users_email",
                table: "users");

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
                    r.assigned_broker AS assigned_broker_id,
                    r.assigned_approver AS assigned_approver_id,
                    r.status,
                    r.note,
                    r.started_at,
                    r.created_at,
                    r.updated_at,
                    d.start_date,
                    c.weekly_cost,
                    p.weekly_payment,
                    r.comment
                FROM
                    referrals AS r
                LEFT JOIN
                    care_package_start_dates AS d ON r.id = d.referral_id
                LEFT JOIN
                    care_package_weekly_costs AS c ON r.id = c.referral_id
                LEFT JOIN
                    care_package_weekly_payments AS p ON r.id = p.referral_id
            ");

            migrationBuilder.Sql("DROP VIEW care_package_name");

        }
    }
}
