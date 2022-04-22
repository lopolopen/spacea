using Microsoft.EntityFrameworkCore.Migrations;

namespace SpaceA.WebApi.Migrations
{
    public partial class DataSeed : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "password",
                table: "members",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "salt",
                table: "members",
                maxLength: 6,
                nullable: true);

            migrationBuilder.InsertData(
                table: "members",
                columns: new[] { "id", "account_name", "avatar_uid", "disabled", "first_name", "last_name", "ming", "password", "refresh_token", "salt", "xing" },
                values: new object[] { 1u, "admin", null, false, "Admin", null, null, "hTAHNb8aFdmxHFE/g7P2wmGTzcE=", null, "ZqSC5", null });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "members",
                keyColumn: "id",
                keyValue: 1u);

            migrationBuilder.DropColumn(
                name: "password",
                table: "members");

            migrationBuilder.DropColumn(
                name: "salt",
                table: "members");
        }
    }
}
