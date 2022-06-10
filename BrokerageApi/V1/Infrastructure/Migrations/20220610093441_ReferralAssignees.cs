using Microsoft.EntityFrameworkCore.Migrations;

namespace V1.Infrastructure.Migrations
{
    public partial class ReferralAssignees : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "assigned_broker",
                table: "referrals",
                newName: "assigned_broker_email");

            migrationBuilder.RenameColumn(
                name: "assigned_approver",
                table: "referrals",
                newName: "assigned_approver_email");

            migrationBuilder.CreateIndex(
                name: "ix_referrals_assigned_approver_email",
                table: "referrals",
                column: "assigned_approver_email");

            migrationBuilder.CreateIndex(
                name: "ix_referrals_assigned_broker_email",
                table: "referrals",
                column: "assigned_broker_email");

            migrationBuilder.AddForeignKey(
                name: "fk_referrals_users_assigned_approver_id",
                table: "referrals",
                column: "assigned_approver_email",
                principalTable: "users",
                principalColumn: "email",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_referrals_users_assigned_broker_id",
                table: "referrals",
                column: "assigned_broker_email",
                principalTable: "users",
                principalColumn: "email",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_referrals_users_assigned_approver_id",
                table: "referrals");

            migrationBuilder.DropForeignKey(
                name: "fk_referrals_users_assigned_broker_id",
                table: "referrals");

            migrationBuilder.DropIndex(
                name: "ix_referrals_assigned_approver_email",
                table: "referrals");

            migrationBuilder.DropIndex(
                name: "ix_referrals_assigned_broker_email",
                table: "referrals");

            migrationBuilder.RenameColumn(
                name: "assigned_broker_email",
                table: "referrals",
                newName: "assigned_broker");

            migrationBuilder.RenameColumn(
                name: "assigned_approver_email",
                table: "referrals",
                newName: "assigned_approver");
        }
    }
}
