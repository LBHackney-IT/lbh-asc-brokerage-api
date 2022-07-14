using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace V1.Infrastructure.Migrations
{
    public partial class AddUniquePartialIndexOnReferralStatusAndSocialCareId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_referrals_social_care_id",
                table: "referrals",
                column: "social_care_id",
                unique: true,
                filter: "status = 'in_progress' OR status = 'awaiting_approval'");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_referrals_social_care_id",
                table: "referrals");
        }
    }
}
