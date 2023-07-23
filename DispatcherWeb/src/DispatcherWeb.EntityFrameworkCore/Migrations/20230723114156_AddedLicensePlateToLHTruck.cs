using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    public partial class AddedLicensePlateToLHTruck : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LicensePlate",
                table: "Truck",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LicensePlate",
                table: "Truck");
        }
    }
}
