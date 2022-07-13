using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace V1.Infrastructure.Migrations
{
    public partial class AddCostAndPaymentOperationsToElementType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "cost_operation",
                table: "element_types",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "payment_operation",
                table: "element_types",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "cost_operation",
                table: "element_types");

            migrationBuilder.DropColumn(
                name: "payment_operation",
                table: "element_types");
        }
    }
}
