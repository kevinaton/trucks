using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    public partial class FreightRatetoPayDriversColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PayDriversFreightRate",
                table: "QuoteService",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PayDriversFreightRate",
                table: "ProjectService",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PayDriversFreightRate",
                table: "OrderLine",
                type: "decimal(18,2)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PayDriversFreightRate",
                table: "QuoteService");

            migrationBuilder.DropColumn(
                name: "PayDriversFreightRate",
                table: "ProjectService");

            migrationBuilder.DropColumn(
                name: "PayDriversFreightRate",
                table: "OrderLine");
        }
    }
}
