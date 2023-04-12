using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    public partial class ColumnRename_FreightRateToPayDrivers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PayDriversFreightRate",
                table: "QuoteService",
                newName: "FreightRateToPayDrivers");

            migrationBuilder.RenameColumn(
                name: "PayDriversFreightRate",
                table: "ProjectService",
                newName: "FreightRateToPayDrivers");

            migrationBuilder.RenameColumn(
                name: "PayDriversFreightRate",
                table: "OrderLine",
                newName: "FreightRateToPayDrivers");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FreightRateToPayDrivers",
                table: "QuoteService",
                newName: "PayDriversFreightRate");

            migrationBuilder.RenameColumn(
                name: "FreightRateToPayDrivers",
                table: "ProjectService",
                newName: "PayDriversFreightRate");

            migrationBuilder.RenameColumn(
                name: "FreightRateToPayDrivers",
                table: "OrderLine",
                newName: "PayDriversFreightRate");
        }
    }
}
