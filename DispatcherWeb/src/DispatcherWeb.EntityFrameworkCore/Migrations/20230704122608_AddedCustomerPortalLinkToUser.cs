using DispatcherWeb.Helpers;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    public partial class AddedCustomerPortalLinkToUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "CustomerContact",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "CustomerContact",
                type: "nvarchar(50)",
                maxLength: 50,
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

            var sql = this.ReadSql();
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
        }
    }
}
