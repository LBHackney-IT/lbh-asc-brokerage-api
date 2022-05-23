using Microsoft.EntityFrameworkCore.Migrations;

namespace V1.Infrastructure.Migrations
{
    public partial class AddSchemaChangesForSuspensions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_suspension",
                table: "elements",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "suspended_element_id",
                table: "elements",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_elements_suspended_element_id",
                table: "elements",
                column: "suspended_element_id");

            migrationBuilder.AddForeignKey(
                name: "fk_elements_elements_suspended_element_id",
                table: "elements",
                column: "suspended_element_id",
                principalTable: "elements",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_elements_elements_suspended_element_id",
                table: "elements");

            migrationBuilder.DropIndex(
                name: "ix_elements_suspended_element_id",
                table: "elements");

            migrationBuilder.DropColumn(
                name: "is_suspension",
                table: "elements");

            migrationBuilder.DropColumn(
                name: "suspended_element_id",
                table: "elements");
        }
    }
}
