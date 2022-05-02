using Microsoft.EntityFrameworkCore.Migrations;

namespace SpaceA.WebApi.Migrations
{
    public partial class x1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "desc",
                table: "work_items");

            migrationBuilder.DropColumn(
                name: "desc",
                table: "work_item_histories");

            migrationBuilder.DropColumn(
                name: "desc",
                table: "teams");

            migrationBuilder.DropColumn(
                name: "desc",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "desc",
                table: "project_histories");

            migrationBuilder.DropColumn(
                name: "desc",
                table: "groups");

            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "work_items",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "work_item_histories",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "teams",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "projects",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "project_histories",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "avatar_uid",
                table: "members",
                maxLength: 32,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(8) CHARACTER SET utf8mb4",
                oldMaxLength: 8,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "groups",
                maxLength: 256,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "members",
                keyColumn: "id",
                keyValue: 1u,
                columns: new[] { "password", "salt" },
                values: new object[] { "RsVfIR1ZYl7+bVYmg/PjRqviNpw=", "wWHQd3" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "description",
                table: "work_items");

            migrationBuilder.DropColumn(
                name: "description",
                table: "work_item_histories");

            migrationBuilder.DropColumn(
                name: "description",
                table: "teams");

            migrationBuilder.DropColumn(
                name: "description",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "description",
                table: "project_histories");

            migrationBuilder.DropColumn(
                name: "description",
                table: "groups");

            migrationBuilder.AddColumn<string>(
                name: "desc",
                table: "work_items",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "desc",
                table: "work_item_histories",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "desc",
                table: "teams",
                type: "varchar(256) CHARACTER SET utf8mb4",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "desc",
                table: "projects",
                type: "varchar(256) CHARACTER SET utf8mb4",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "desc",
                table: "project_histories",
                type: "varchar(256) CHARACTER SET utf8mb4",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "avatar_uid",
                table: "members",
                type: "varchar(8) CHARACTER SET utf8mb4",
                maxLength: 8,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 32,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "desc",
                table: "groups",
                type: "varchar(256) CHARACTER SET utf8mb4",
                maxLength: 256,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "members",
                keyColumn: "id",
                keyValue: 1u,
                columns: new[] { "password", "salt" },
                values: new object[] { "hTAHNb8aFdmxHFE/g7P2wmGTzcE=", "ZqSC5" });
        }
    }
}
