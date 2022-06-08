using Microsoft.EntityFrameworkCore.Migrations;

namespace V1.Infrastructure.Migrations
{
    public partial class AddCedarSiteToProvider : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "cedar_site",
                table: "providers",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_providers_cedar_number_cedar_site",
                table: "providers",
                columns: new[] { "cedar_number", "cedar_site" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_providers_cedar_number_cedar_site",
                table: "providers");

            migrationBuilder.DropColumn(
                name: "cedar_site",
                table: "providers");
        }
    }
}
