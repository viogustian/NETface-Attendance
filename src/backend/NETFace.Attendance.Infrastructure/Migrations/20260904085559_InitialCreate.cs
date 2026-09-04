using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NETFace.Attendance.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AttendanceSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DepartmentName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttendanceSessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FullName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    IsAdmin = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RecognitionLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: true),
                    MatchedEmployeeCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsSuccess = table.Column<bool>(type: "boolean", nullable: false),
                    Confidence = table.Column<double>(type: "double precision", nullable: false),
                    ProcessingTimeMs = table.Column<long>(type: "bigint", nullable: false),
                    AttemptedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecognitionLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AttendanceEntry",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EmployeeName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    MarkedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    AttendanceSessionId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttendanceEntry", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AttendanceEntry_AttendanceSessions_AttendanceSessionId",
                        column: x => x.AttendanceSessionId,
                        principalTable: "AttendanceSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FaceEmbedding",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Vector = table.Column<float[]>(type: "real[]", nullable: false),
                    CapturedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FaceEmbedding", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FaceEmbedding_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceEntry_AttendanceSessionId",
                table: "AttendanceEntry",
                column: "AttendanceSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_EmployeeCode",
                table: "Employees",
                column: "EmployeeCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FaceEmbedding_EmployeeId",
                table: "FaceEmbedding",
                column: "EmployeeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AttendanceEntry");

            migrationBuilder.DropTable(
                name: "FaceEmbedding");

            migrationBuilder.DropTable(
                name: "RecognitionLogs");

            migrationBuilder.DropTable(
                name: "AttendanceSessions");

            migrationBuilder.DropTable(
                name: "Employees");
        }
    }
}
