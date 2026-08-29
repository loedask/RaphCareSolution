using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using RaphCare.Persistence;

#nullable disable

namespace RaphCare.Persistence.Migrations;

[DbContext(typeof(ClinicalDbContext))]
[Migration("20260829090000_AddCollectionWaitingDisplay")]
public partial class AddCollectionWaitingDisplay : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "CollectionDisplayToken",
            table: "Clinics",
            type: "nvarchar(12)",
            maxLength: 12,
            nullable: true);

        migrationBuilder.AddColumn<DateTime>(
            name: "CalledAt",
            table: "Prescription",
            type: "datetime2",
            nullable: true);

        migrationBuilder.AddColumn<DateTime>(
            name: "CalledAt",
            table: "LabRequest",
            type: "datetime2",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_Clinics_CollectionDisplayToken",
            table: "Clinics",
            column: "CollectionDisplayToken",
            unique: true,
            filter: "[CollectionDisplayToken] IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_Prescription_CalledAt",
            table: "Prescription",
            column: "CalledAt");

        migrationBuilder.CreateIndex(
            name: "IX_LabRequest_CalledAt",
            table: "LabRequest",
            column: "CalledAt");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "IX_Clinics_CollectionDisplayToken", table: "Clinics");
        migrationBuilder.DropIndex(name: "IX_Prescription_CalledAt", table: "Prescription");
        migrationBuilder.DropIndex(name: "IX_LabRequest_CalledAt", table: "LabRequest");
        migrationBuilder.DropColumn(name: "CollectionDisplayToken", table: "Clinics");
        migrationBuilder.DropColumn(name: "CalledAt", table: "Prescription");
        migrationBuilder.DropColumn(name: "CalledAt", table: "LabRequest");
    }
}
