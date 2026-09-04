using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using RaphCare.Persistence;

#nullable disable

namespace RaphCare.Persistence.Migrations;

[DbContext(typeof(ClinicalDbContext))]
[Migration("20260904180000_MentalHealthTherapyAndAiNotes")]
public partial class MentalHealthTherapyAndAiNotes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "AllowAiMentalHealthNotes",
            table: "Clinics",
            type: "bit",
            nullable: false,
            defaultValue: true);

        migrationBuilder.CreateTable(
            name: "BehavioralCarePlans",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ClinicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_BehavioralCarePlans", x => x.Id);
                table.ForeignKey(
                    name: "FK_BehavioralCarePlans_Clinics_ClinicId",
                    column: x => x.ClinicId,
                    principalTable: "Clinics",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_BehavioralCarePlans_Patients_PatientId",
                    column: x => x.PatientId,
                    principalTable: "Patients",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "TherapySessions",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ClinicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                TherapistId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                TeleSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                SessionStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                SessionEnd = table.Column<DateTime>(type: "datetime2", nullable: true),
                SessionType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                Summary = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                IsConfidential = table.Column<bool>(type: "bit", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TherapySessions", x => x.Id);
                table.ForeignKey(
                    name: "FK_TherapySessions_Clinics_ClinicId",
                    column: x => x.ClinicId,
                    principalTable: "Clinics",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_TherapySessions_Patients_PatientId",
                    column: x => x.PatientId,
                    principalTable: "Patients",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "TherapyGoals",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                BehavioralCarePlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                GoalDescription = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                TargetDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TherapyGoals", x => x.Id);
                table.ForeignKey(
                    name: "FK_TherapyGoals_BehavioralCarePlans_BehavioralCarePlanId",
                    column: x => x.BehavioralCarePlanId,
                    principalTable: "BehavioralCarePlans",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "CrisisFlags",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                TherapySessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                RiskLevel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                FlaggedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                IsResolved = table.Column<bool>(type: "bit", nullable: false),
                ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CrisisFlags", x => x.Id);
                table.ForeignKey(
                    name: "FK_CrisisFlags_TherapySessions_TherapySessionId",
                    column: x => x.TherapySessionId,
                    principalTable: "TherapySessions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "TherapyNotes",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                TherapySessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Notes = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                IsPrivate = table.Column<bool>(type: "bit", nullable: false),
                RecordedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TherapyNotes", x => x.Id);
                table.ForeignKey(
                    name: "FK_TherapyNotes_TherapySessions_TherapySessionId",
                    column: x => x.TherapySessionId,
                    principalTable: "TherapySessions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(name: "IX_BehavioralCarePlans_ClinicId", table: "BehavioralCarePlans", column: "ClinicId");
        migrationBuilder.CreateIndex(name: "IX_BehavioralCarePlans_PatientId", table: "BehavioralCarePlans", column: "PatientId");
        migrationBuilder.CreateIndex(name: "IX_TherapyGoals_BehavioralCarePlanId", table: "TherapyGoals", column: "BehavioralCarePlanId");
        migrationBuilder.CreateIndex(name: "IX_TherapySessions_ClinicId", table: "TherapySessions", column: "ClinicId");
        migrationBuilder.CreateIndex(name: "IX_TherapySessions_PatientId", table: "TherapySessions", column: "PatientId");
        migrationBuilder.CreateIndex(name: "IX_TherapySessions_ClinicId_SessionStart", table: "TherapySessions", columns: new[] { "ClinicId", "SessionStart" });
        migrationBuilder.CreateIndex(name: "IX_CrisisFlags_TherapySessionId", table: "CrisisFlags", column: "TherapySessionId");
        migrationBuilder.CreateIndex(name: "IX_CrisisFlags_PatientId", table: "CrisisFlags", column: "PatientId");
        migrationBuilder.CreateIndex(name: "IX_TherapyNotes_TherapySessionId", table: "TherapyNotes", column: "TherapySessionId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "TherapyNotes");
        migrationBuilder.DropTable(name: "CrisisFlags");
        migrationBuilder.DropTable(name: "TherapyGoals");
        migrationBuilder.DropTable(name: "TherapySessions");
        migrationBuilder.DropTable(name: "BehavioralCarePlans");
        migrationBuilder.DropColumn(name: "AllowAiMentalHealthNotes", table: "Clinics");
    }
}
