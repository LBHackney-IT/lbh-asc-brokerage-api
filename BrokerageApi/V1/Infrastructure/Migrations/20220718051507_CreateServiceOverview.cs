using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace V1.Infrastructure.Migrations
{
    public partial class CreateServiceOverview : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE VIEW service_overview_costs AS
                SELECT
                    et.service_id,
                    e.social_care_id,
                    SUM(e.cost * et.cost_operation) AS weekly_cost,
                    SUM(e.cost * et.payment_operation) AS weekly_payment,
                    SUM(e.cost * et.cost_operation) * 52 AS annual_cost
                FROM
                    elements AS e
                INNER JOIN
                    element_types AS et ON e.element_type_id = et.id
                INNER JOIN
                    services AS s ON et.service_id = s.id
                WHERE
                    e.internal_status = 'approved'
                AND
                    e.start_date <= CURRENT_DATE
                AND
                    (e.end_date IS NULL OR e.end_date > CURRENT_DATE)
                GROUP BY
                    e.social_care_id, et.service_id;
            ");

            migrationBuilder.Sql(@"
                CREATE VIEW service_overview_dates AS
                SELECT
                    et.service_id,
                    e.social_care_id,
                    MIN(e.start_date) AS start_date,
                    NULLIF(MAX(COALESCE(e.end_date, '3000-01-01')), '3000-01-01') AS end_date
                FROM
                    elements AS e
                INNER JOIN
                    element_types AS et ON e.element_type_id = et.id
                WHERE
                    e.internal_status = 'approved'
                AND
                    et.type = 'service'
                GROUP BY
                    e.social_care_id, et.service_id;
            ");

            migrationBuilder.Sql(@"
                CREATE VIEW service_overviews AS
                SELECT
                    d.social_care_id,
                    s.id,
                    s.name,
                    d.start_date,
                    d.end_date,
                    c.weekly_cost,
                    c.weekly_payment,
                    c.annual_cost
                FROM
                    services AS s
                INNER JOIN
                    service_overview_dates AS d ON s.id = d.service_id
                LEFT JOIN
                    service_overview_costs AS c
                ON
                    d.service_id = c.service_id AND d.social_care_id = c.social_care_id
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW service_overviews");
            migrationBuilder.Sql("DROP VIEW service_overview_costs");
            migrationBuilder.Sql("DROP VIEW service_overview_dates");
        }
    }
}
