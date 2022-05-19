using Microsoft.EntityFrameworkCore.Migrations;

namespace V1.Infrastructure.Migrations
{
    public partial class AddAssignedApproverRenameAssignedToToAssignedBroker : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "assigned_to",
                table: "referrals",
                newName: "assigned_broker");

            migrationBuilder.AddColumn<string>(
                name: "assigned_approver",
                table: "referrals",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "assigned_approver",
                table: "referrals");

            migrationBuilder.RenameColumn(
                name: "assigned_broker",
                table: "referrals",
                newName: "assigned_to");
        }
    }
}
