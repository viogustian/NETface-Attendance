using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NETFace.Attendance.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEnrollmentLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EnrollmentLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Action = table.Column<string>(type: "text", nullable: false),
                    IsSuccess = table.Column<bool>(type: "boolean", nullable: false),
                    NumberOfPhotos = table.Column<int>(type: "integer", nullable: false),
                    NumberOfEmbeddingsCreatedOrRemoved = table.Column<int>(type: "integer", nullable: false),
                    PerformedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FailureReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnrollmentLogs", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EnrollmentLogs");
        }
    }
}
