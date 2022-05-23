using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

namespace V1.Infrastructure.Migrations
{
    public partial class CreateDateNumbersTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "dates",
                columns: table => new
                {
                    value = table.Column<LocalDate>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_dates", x => x.value);
                });

            migrationBuilder.Sql(@"
                INSERT INTO dates SELECT
                GENERATE_SERIES('2015-01-01'::date, '2035-12-31'::date, '1 day'::interval)
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "dates");
        }
    }
}
