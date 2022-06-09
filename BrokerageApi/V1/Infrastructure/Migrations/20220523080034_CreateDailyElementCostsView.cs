using Microsoft.EntityFrameworkCore.Migrations;

namespace V1.Infrastructure.Migrations
{
    public partial class CreateDailyElementCostsView : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE VIEW daily_element_costs AS
                SELECT
                    e.id,
                    d.value AS date,
                    e.daily_costs[EXTRACT(isodow FROM d.value)] AS cost
                FROM
                    elements AS e
                INNER JOIN
                    dates AS d ON (e.start_date <= d.value AND (e.end_date >= d.value OR e.end_date IS NULL))
                WHERE e.internal_status = 'approved'
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW daily_element_costs");
        }
    }
}
