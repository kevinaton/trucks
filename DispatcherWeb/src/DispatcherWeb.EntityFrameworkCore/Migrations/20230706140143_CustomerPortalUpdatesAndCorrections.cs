using DispatcherWeb.Helpers;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    public partial class AddedCustomerPortalLinkToUserAndOtherUpdates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "CustomerContact",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "CustomerContact",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "CustomerContact",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "CustomerContactId",
                table: "AbpUsers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AbpUsers_CustomerContactId",
                table: "AbpUsers",
                column: "CustomerContactId");

            migrationBuilder.AddForeignKey(
                name: "FK_AbpUsers_CustomerContact_CustomerContactId",
                table: "AbpUsers",
                column: "CustomerContactId",
                principalTable: "CustomerContact",
                principalColumn: "Id");

            migrationBuilder.Sql(@"DECLARE @DB_NAME nvarchar(128) = DB_NAME();
EXEC ('ALTER DATABASE [' + @DB_NAME + ']  SET COMPATIBILITY_LEVEL = 130');", true);

            var sql = this.ReadSql();
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DECLARE @DB_NAME nvarchar(128) = DB_NAME();
EXEC ('ALTER DATABASE [' + @DB_NAME + ']  SET COMPATIBILITY_LEVEL = 100');", true);

            migrationBuilder.DropForeignKey(
                name: "FK_AbpUsers_CustomerContact_CustomerContactId",
                table: "AbpUsers");

            migrationBuilder.DropIndex(
                name: "IX_AbpUsers_CustomerContactId",
                table: "AbpUsers");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "CustomerContact");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "CustomerContact");

            migrationBuilder.DropColumn(
                name: "CustomerContactId",
                table: "AbpUsers");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "CustomerContact",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128,
                oldNullable: true);
        }
    }
}
