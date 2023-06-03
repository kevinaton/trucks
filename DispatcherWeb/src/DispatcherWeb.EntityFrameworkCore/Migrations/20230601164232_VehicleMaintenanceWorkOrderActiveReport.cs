using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    public partial class VehicleMaintenanceWorkOrderActiveReport : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Path",
                table: "ActiveReport",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "ActiveReport",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);

            var commandText = @"
SET IDENTITY_INSERT [dbo].[ActiveReport] ON;
INSERT INTO [dbo].[ActiveReport] (ID, [Name], [Description], [Path], CategoryId, CreationTime, IsDeleted)
VALUES (2, 'VehicleMaintenanceWorkOrderReport', 'Vehicle Maintenance Work Order Report', 'VehicleMaintenanceWorkOrderReport', 1, GETDATE(), 0);
SET IDENTITY_INSERT [dbo].[ActiveReport] OFF;
";

            migrationBuilder.Sql(commandText);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Path",
                table: "ActiveReport",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(80)",
                oldMaxLength: 80);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "ActiveReport",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(80)",
                oldMaxLength: 80);
        }
    }
}
