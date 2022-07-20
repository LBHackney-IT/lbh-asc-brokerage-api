using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace V1.Infrastructure.Migrations
{
    public partial class RenameReferralAmendmentsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_referral_amendment_referrals_referral_id",
                table: "referral_amendment");

            migrationBuilder.DropPrimaryKey(
                name: "pk_referral_amendment",
                table: "referral_amendment");

            migrationBuilder.RenameTable(
                name: "referral_amendment",
                newName: "referral_amendments");

            migrationBuilder.RenameIndex(
                name: "ix_referral_amendment_referral_id",
                table: "referral_amendments",
                newName: "ix_referral_amendments_referral_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_referral_amendments",
                table: "referral_amendments",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_referral_amendments_referrals_referral_id",
                table: "referral_amendments",
                column: "referral_id",
                principalTable: "referrals",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_referral_amendments_referrals_referral_id",
                table: "referral_amendments");

            migrationBuilder.DropPrimaryKey(
                name: "pk_referral_amendments",
                table: "referral_amendments");

            migrationBuilder.RenameTable(
                name: "referral_amendments",
                newName: "referral_amendment");

            migrationBuilder.RenameIndex(
                name: "ix_referral_amendments_referral_id",
                table: "referral_amendment",
                newName: "ix_referral_amendment_referral_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_referral_amendment",
                table: "referral_amendment",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_referral_amendment_referrals_referral_id",
                table: "referral_amendment",
                column: "referral_id",
                principalTable: "referrals",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
