using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Parkking.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FechaCobroabono : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "FechaInicioCobro",
                table: "AbonoCocheras",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FechaInicioCobro",
                table: "AbonoCocheras");
        }
    }
}
