using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NETFace.Attendance.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddClockInOutAndSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ClockInTime",
                table: "AttendanceEntry",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ClockOutTime",
                table: "AttendanceEntry",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "TotalWorkHours",
                table: "AttendanceEntry",
                type: "double precision",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SystemSettings",
                columns: table => new
                {
                    Key = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemSettings", x => x.Key);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "ClockInTime",
                table: "AttendanceEntry");

            migrationBuilder.DropColumn(
                name: "ClockOutTime",
                table: "AttendanceEntry");

            migrationBuilder.DropColumn(
                name: "TotalWorkHours",
                table: "AttendanceEntry");
        }
    }
}
