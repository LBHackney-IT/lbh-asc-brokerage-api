using Microsoft.EntityFrameworkCore.Migrations;

namespace V1.Infrastructure.Migrations
{
    public partial class AddCommentsToElementAndCarePackage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "comment",
                table: "referrals",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "comment",
                table: "elements",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "comment",
                table: "referrals");

            migrationBuilder.DropColumn(
                name: "comment",
                table: "elements");
        }
    }
}
