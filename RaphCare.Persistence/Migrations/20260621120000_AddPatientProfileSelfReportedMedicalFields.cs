using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using RaphCare.Persistence;

#nullable disable

namespace RaphCare.Persistence.Migrations;

[DbContext(typeof(ClinicalDbContext))]
[Migration("20260621120000_AddPatientProfileSelfReportedMedicalFields")]
public partial class AddPatientProfileSelfReportedMedicalFields : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "PrimaryCareProviderName",
            table: "PatientProfile",
            type: "nvarchar(256)",
            maxLength: 256,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "SelfReportedAllergies",
            table: "PatientProfile",
            type: "nvarchar(2000)",
            maxLength: 2000,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "SelfReportedChronicConditions",
            table: "PatientProfile",
            type: "nvarchar(2000)",
            maxLength: 2000,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "SelfReportedMedications",
            table: "PatientProfile",
            type: "nvarchar(2000)",
            maxLength: 2000,
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "PrimaryCareProviderName", table: "PatientProfile");
        migrationBuilder.DropColumn(name: "SelfReportedAllergies", table: "PatientProfile");
        migrationBuilder.DropColumn(name: "SelfReportedChronicConditions", table: "PatientProfile");
        migrationBuilder.DropColumn(name: "SelfReportedMedications", table: "PatientProfile");
    }
}
