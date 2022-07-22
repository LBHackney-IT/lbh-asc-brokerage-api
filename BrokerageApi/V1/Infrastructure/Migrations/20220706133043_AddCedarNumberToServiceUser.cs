using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NpgsqlTypes;

#nullable disable

namespace V1.Infrastructure.Migrations
{
    public partial class AddCedarNumberToServiceUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //this is happening now because I had modified the BrokerageContext after creating this field with another migration, I think
            migrationBuilder.AlterColumn<NpgsqlTsVector>(
                name: "name_search_vector",
                table: "service_users",
                type: "tsvector",
                nullable: true,
                oldClrType: typeof(NpgsqlTsVector),
                oldType: "tsvector",
                oldNullable: true)
                .Annotation("Npgsql:TsVectorConfig", "simple")
                .Annotation("Npgsql:TsVectorProperties", new[] { "service_user_name" });

            migrationBuilder.AddColumn<string>(
                name: "cedar_number",
                table: "service_users",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "cedar_number",
                table: "service_users");

            migrationBuilder.AlterColumn<NpgsqlTsVector>(
                name: "name_search_vector",
                table: "service_users",
                type: "tsvector",
                nullable: true,
                oldClrType: typeof(NpgsqlTsVector),
                oldType: "tsvector",
                oldNullable: true)
                .OldAnnotation("Npgsql:TsVectorConfig", "simple")
                .OldAnnotation("Npgsql:TsVectorProperties", new[] { "service_user_name" });
        }
    }
}
