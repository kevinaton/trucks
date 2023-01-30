﻿using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class AddNumberOfLoadsToFinishToDispatchTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NumberOfLoadsToFinish",
                table: "Dispatch",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NumberOfLoadsToFinish",
                table: "Dispatch");
        }
    }
}
