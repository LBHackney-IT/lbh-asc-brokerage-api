using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace V1.Infrastructure.Migrations
{
    public partial class AddDailyCostsToElement : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<decimal>>(
                name: "daily_costs",
                table: "elements",
                type: "numeric[]",
                nullable: true,
                computedColumnSql: "ARRAY[COALESCE((monday->>'Cost')::numeric, 0), COALESCE((tuesday->>'Cost')::numeric, 0), COALESCE((wednesday->>'Cost')::numeric, 0), COALESCE((thursday->>'Cost')::numeric, 0), COALESCE((friday->>'Cost')::numeric, 0), COALESCE((saturday->>'Cost')::numeric, 0), COALESCE((sunday->>'Cost')::numeric, 0)]",
                stored: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "daily_costs",
                table: "elements");
        }
    }
}
