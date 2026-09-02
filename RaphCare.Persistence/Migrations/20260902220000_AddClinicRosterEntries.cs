using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using RaphCare.Persistence;

#nullable disable

namespace RaphCare.Persistence.Migrations;

[DbContext(typeof(ClinicalDbContext))]
[Migration("20260902220000_AddClinicRosterEntries")]
public partial class AddClinicRosterEntries : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ClinicRosterEntries",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ClinicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                DutyDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                ShiftLabel = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                Note = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                CreatedByApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ClinicRosterEntries", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_ClinicRosterEntries_ClinicId_DutyDate",
            table: "ClinicRosterEntries",
            columns: new[] { "ClinicId", "DutyDate" });

        migrationBuilder.CreateIndex(
            name: "IX_ClinicRosterEntries_ClinicId_DutyDate_ApplicationUserId_ShiftLabel",
            table: "ClinicRosterEntries",
            columns: new[] { "ClinicId", "DutyDate", "ApplicationUserId", "ShiftLabel" },
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "ClinicRosterEntries");
    }
}
