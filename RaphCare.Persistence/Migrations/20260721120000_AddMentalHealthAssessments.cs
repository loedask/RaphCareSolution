using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using RaphCare.Persistence;

#nullable disable

namespace RaphCare.Persistence.Migrations;

[DbContext(typeof(ClinicalDbContext))]
[Migration("20260721120000_AddMentalHealthAssessments")]
public partial class AddMentalHealthAssessments : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "MentalHealthAssessments",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ClinicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                AssessmentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                ConductedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                TotalScore = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                SeverityLevel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                IsAIEnhanced = table.Column<bool>(type: "bit", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_MentalHealthAssessments", x => x.Id);
                table.ForeignKey(
                    name: "FK_MentalHealthAssessments_Clinics_ClinicId",
                    column: x => x.ClinicId,
                    principalTable: "Clinics",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_MentalHealthAssessments_Patients_PatientId",
                    column: x => x.PatientId,
                    principalTable: "Patients",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(name: "IX_MentalHealthAssessments_ClinicId", table: "MentalHealthAssessments", column: "ClinicId");
        migrationBuilder.CreateIndex(name: "IX_MentalHealthAssessments_PatientId", table: "MentalHealthAssessments", column: "PatientId");
        migrationBuilder.CreateIndex(
            name: "IX_MentalHealthAssessments_ClinicId_ConductedAt",
            table: "MentalHealthAssessments",
            columns: new[] { "ClinicId", "ConductedAt" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "MentalHealthAssessments");
    }
}
