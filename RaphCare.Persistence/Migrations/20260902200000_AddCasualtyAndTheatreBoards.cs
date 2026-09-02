using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using RaphCare.Persistence;

#nullable disable

namespace RaphCare.Persistence.Migrations;

[DbContext(typeof(ClinicalDbContext))]
[Migration("20260902200000_AddCasualtyAndTheatreBoards")]
public partial class AddCasualtyAndTheatreBoards : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "CasualtyDisplayToken",
            table: "Clinics",
            type: "nvarchar(12)",
            maxLength: 12,
            nullable: true);

        migrationBuilder.CreateTable(
            name: "CasualtyTickets",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ClinicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                QueueCode = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: false),
                TriageLevel = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                ChiefComplaint = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                Status = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                ArrivedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                CalledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                CreatedByApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CasualtyTickets", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "TheatreCases",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ClinicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ScheduledStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                ScheduledEnd = table.Column<DateTime>(type: "datetime2", nullable: true),
                ProcedureName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                TheatreName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                SurgeonName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                Status = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                CreatedByApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TheatreCases", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Clinics_CasualtyDisplayToken",
            table: "Clinics",
            column: "CasualtyDisplayToken",
            unique: true,
            filter: "[CasualtyDisplayToken] IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_CasualtyTickets_CalledAt",
            table: "CasualtyTickets",
            column: "CalledAt");

        migrationBuilder.CreateIndex(
            name: "IX_CasualtyTickets_ClinicId",
            table: "CasualtyTickets",
            column: "ClinicId");

        migrationBuilder.CreateIndex(
            name: "IX_CasualtyTickets_ClinicId_Status",
            table: "CasualtyTickets",
            columns: new[] { "ClinicId", "Status" });

        migrationBuilder.CreateIndex(
            name: "IX_CasualtyTickets_QueueCode",
            table: "CasualtyTickets",
            column: "QueueCode");

        migrationBuilder.CreateIndex(
            name: "IX_TheatreCases_ClinicId",
            table: "TheatreCases",
            column: "ClinicId");

        migrationBuilder.CreateIndex(
            name: "IX_TheatreCases_ClinicId_ScheduledStart",
            table: "TheatreCases",
            columns: new[] { "ClinicId", "ScheduledStart" });

        migrationBuilder.CreateIndex(
            name: "IX_TheatreCases_PatientId",
            table: "TheatreCases",
            column: "PatientId");

        migrationBuilder.CreateIndex(
            name: "IX_TheatreCases_Status",
            table: "TheatreCases",
            column: "Status");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "CasualtyTickets");
        migrationBuilder.DropTable(name: "TheatreCases");
        migrationBuilder.DropIndex(name: "IX_Clinics_CasualtyDisplayToken", table: "Clinics");
        migrationBuilder.DropColumn(name: "CasualtyDisplayToken", table: "Clinics");
    }
}
