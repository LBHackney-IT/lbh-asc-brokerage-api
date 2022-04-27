using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

namespace V1.Infrastructure.Migrations
{
    public partial class SwitchToNodaTime : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Instant>(
                name: "updated_at",
                table: "users",
                type: "timestamp",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<Instant>(
                name: "created_at",
                table: "users",
                type: "timestamp",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<Instant>(
                name: "urgent_since",
                table: "referrals",
                type: "timestamp",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<Instant>(
                name: "updated_at",
                table: "referrals",
                type: "timestamp",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<Instant>(
                name: "started_at",
                table: "referrals",
                type: "timestamp",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<Instant>(
                name: "created_at",
                table: "referrals",
                type: "timestamp",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<Instant>(
                name: "updated_at",
                table: "providers",
                type: "timestamp",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<Instant>(
                name: "created_at",
                table: "providers",
                type: "timestamp",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                table: "users",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(Instant),
                oldType: "timestamp");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "users",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(Instant),
                oldType: "timestamp");

            migrationBuilder.AlterColumn<DateTime>(
                name: "urgent_since",
                table: "referrals",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(Instant),
                oldType: "timestamp",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                table: "referrals",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(Instant),
                oldType: "timestamp");

            migrationBuilder.AlterColumn<DateTime>(
                name: "started_at",
                table: "referrals",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(Instant),
                oldType: "timestamp",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "referrals",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(Instant),
                oldType: "timestamp");

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                table: "providers",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(Instant),
                oldType: "timestamp");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "providers",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(Instant),
                oldType: "timestamp");
        }
    }
}
