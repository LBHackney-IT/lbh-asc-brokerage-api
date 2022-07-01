using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace V1.Infrastructure.Migrations
{
    public partial class MigrateTimestamps : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("SET TimeZone='UTC'");

            migrationBuilder.Sql("DROP VIEW care_packages");

            migrationBuilder.AlterColumn<Instant>(
                name: "updated_at",
                table: "users",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(Instant),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<Instant>(
                name: "created_at",
                table: "users",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(Instant),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<Instant>(
                name: "updated_at",
                table: "service_users",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(Instant),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<Instant>(
                name: "created_at",
                table: "service_users",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(Instant),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<Instant>(
                name: "urgent_since",
                table: "referrals",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(Instant),
                oldType: "timestamp without time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<Instant>(
                name: "updated_at",
                table: "referrals",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(Instant),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<Instant>(
                name: "started_at",
                table: "referrals",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(Instant),
                oldType: "timestamp without time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<Instant>(
                name: "created_at",
                table: "referrals",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(Instant),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<Instant>(
                name: "requested_at",
                table: "referral_amendment",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(Instant),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<Instant>(
                name: "updated_at",
                table: "providers",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(Instant),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<Instant>(
                name: "created_at",
                table: "providers",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(Instant),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<Instant>(
                name: "updated_at",
                table: "elements",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(Instant),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<Instant>(
                name: "created_at",
                table: "elements",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(Instant),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<Instant>(
                name: "created_at",
                table: "audit_events",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(Instant),
                oldType: "timestamp without time zone");

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
            migrationBuilder.Sql("SET TimeZone='UTC'");

            migrationBuilder.Sql("DROP VIEW care_packages");

            migrationBuilder.AlterColumn<Instant>(
                name: "updated_at",
                table: "users",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(Instant),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<Instant>(
                name: "created_at",
                table: "users",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(Instant),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<Instant>(
                name: "updated_at",
                table: "service_users",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(Instant),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<Instant>(
                name: "created_at",
                table: "service_users",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(Instant),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<Instant>(
                name: "urgent_since",
                table: "referrals",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(Instant),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<Instant>(
                name: "updated_at",
                table: "referrals",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(Instant),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<Instant>(
                name: "started_at",
                table: "referrals",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(Instant),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<Instant>(
                name: "created_at",
                table: "referrals",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(Instant),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<Instant>(
                name: "requested_at",
                table: "referral_amendment",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(Instant),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<Instant>(
                name: "updated_at",
                table: "providers",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(Instant),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<Instant>(
                name: "created_at",
                table: "providers",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(Instant),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<Instant>(
                name: "updated_at",
                table: "elements",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(Instant),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<Instant>(
                name: "created_at",
                table: "elements",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(Instant),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<Instant>(
                name: "created_at",
                table: "audit_events",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(Instant),
                oldType: "timestamp with time zone");

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
    }
}
