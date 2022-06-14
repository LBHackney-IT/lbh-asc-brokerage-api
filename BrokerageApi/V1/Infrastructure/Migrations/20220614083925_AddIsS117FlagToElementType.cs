using Microsoft.EntityFrameworkCore.Migrations;

namespace V1.Infrastructure.Migrations
{
    public partial class AddIsS117FlagToElementType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_s117",
                table: "element_types",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_s117",
                table: "element_types");
        }
    }
}
