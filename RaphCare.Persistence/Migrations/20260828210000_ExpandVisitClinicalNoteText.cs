using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using RaphCare.Persistence;

#nullable disable

namespace RaphCare.Persistence.Migrations;

[DbContext(typeof(ClinicalDbContext))]
[Migration("20260828210000_ExpandVisitClinicalNoteText")]
public partial class ExpandVisitClinicalNoteText : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "Subjective",
            table: "SOAPNote",
            type: "nvarchar(4000)",
            maxLength: 4000,
            nullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "Objective",
            table: "SOAPNote",
            type: "nvarchar(4000)",
            maxLength: 4000,
            nullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "Assessment",
            table: "SOAPNote",
            type: "nvarchar(4000)",
            maxLength: 4000,
            nullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "Plan",
            table: "SOAPNote",
            type: "nvarchar(4000)",
            maxLength: 4000,
            nullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "Notes",
            table: "ClinicalNote",
            type: "nvarchar(4000)",
            maxLength: 4000,
            nullable: false);

        migrationBuilder.AlterColumn<string>(
            name: "Category",
            table: "ClinicalNote",
            type: "nvarchar(100)",
            maxLength: 100,
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "Subjective",
            table: "SOAPNote",
            type: "nvarchar(256)",
            maxLength: 256,
            nullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "Objective",
            table: "SOAPNote",
            type: "nvarchar(256)",
            maxLength: 256,
            nullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "Assessment",
            table: "SOAPNote",
            type: "nvarchar(256)",
            maxLength: 256,
            nullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "Plan",
            table: "SOAPNote",
            type: "nvarchar(256)",
            maxLength: 256,
            nullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "Notes",
            table: "ClinicalNote",
            type: "nvarchar(256)",
            maxLength: 256,
            nullable: false);

        migrationBuilder.AlterColumn<string>(
            name: "Category",
            table: "ClinicalNote",
            type: "nvarchar(256)",
            maxLength: 256,
            nullable: true);
    }
}
