using Microsoft.EntityFrameworkCore.Migrations;

namespace V1.Infrastructure.Migrations
{
    public partial class ParentElement : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_elements_elements_related_element_id",
                table: "elements");

            migrationBuilder.RenameColumn(
                name: "related_element_id",
                table: "elements",
                newName: "parent_element_id");

            migrationBuilder.RenameIndex(
                name: "ix_elements_related_element_id",
                table: "elements",
                newName: "ix_elements_parent_element_id");

            migrationBuilder.AddForeignKey(
                name: "fk_elements_elements_parent_element_id",
                table: "elements",
                column: "parent_element_id",
                principalTable: "elements",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_elements_elements_parent_element_id",
                table: "elements");

            migrationBuilder.RenameColumn(
                name: "parent_element_id",
                table: "elements",
                newName: "related_element_id");

            migrationBuilder.RenameIndex(
                name: "ix_elements_parent_element_id",
                table: "elements",
                newName: "ix_elements_related_element_id");

            migrationBuilder.AddForeignKey(
                name: "fk_elements_elements_related_element_id",
                table: "elements",
                column: "related_element_id",
                principalTable: "elements",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
