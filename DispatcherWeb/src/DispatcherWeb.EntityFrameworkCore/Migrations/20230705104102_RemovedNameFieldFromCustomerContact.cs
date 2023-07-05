using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    public partial class RemovedNameFieldFromCustomerContact : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "CustomerContact");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "CustomerContact",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }
    }
}
