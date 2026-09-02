using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using RaphCare.Persistence;

#nullable disable

namespace RaphCare.Persistence.Migrations;

[DbContext(typeof(ClinicalDbContext))]
[Migration("20260902120000_AddWardNotesAndDischargeSummary")]
public partial class AddWardNotesAndDischargeSummary : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "DischargeSummary",
            table: "InpatientAdmissions",
            type: "nvarchar(4000)",
            maxLength: 4000,
            nullable: true);

        migrationBuilder.AddColumn<Guid>(
            name: "InvoiceId",
            table: "InpatientAdmissions",
            type: "uniqueidentifier",
            nullable: true);

        migrationBuilder.CreateTable(
            name: "InpatientObservations",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ClinicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                AdmissionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                RecordedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                RecordedByApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                Note = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                HeartRate = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: true),
                TemperatureCelsius = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                OxygenSaturation = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                SystolicBp = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: true),
                DiastolicBp = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_InpatientObservations", x => x.Id);
                table.ForeignKey(
                    name: "FK_InpatientObservations_InpatientAdmissions_AdmissionId",
                    column: x => x.AdmissionId,
                    principalTable: "InpatientAdmissions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_InpatientObservations_AdmissionId",
            table: "InpatientObservations",
            column: "AdmissionId");

        migrationBuilder.CreateIndex(
            name: "IX_InpatientObservations_ClinicId",
            table: "InpatientObservations",
            column: "ClinicId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "InpatientObservations");

        migrationBuilder.DropColumn(name: "DischargeSummary", table: "InpatientAdmissions");
        migrationBuilder.DropColumn(name: "InvoiceId", table: "InpatientAdmissions");
    }
}
