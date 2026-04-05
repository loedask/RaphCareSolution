using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using RaphCare.Persistence;

#nullable disable

namespace RaphCare.Persistence.Migrations;

[DbContext(typeof(ClinicalDbContext))]
[Migration("20260403120000_PatientFamilyMembers_Vertical10")]
public partial class PatientFamilyMembers_Vertical10 : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "PatientFamilyMembers",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                OwnerPatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Relationship = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                PhoneNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                LinkedPatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PatientFamilyMembers", x => x.Id);
                table.ForeignKey(
                    name: "FK_PatientFamilyMembers_Patients_LinkedPatientId",
                    column: x => x.LinkedPatientId,
                    principalTable: "Patients",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_PatientFamilyMembers_Patients_OwnerPatientId",
                    column: x => x.OwnerPatientId,
                    principalTable: "Patients",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_PatientFamilyMembers_LinkedPatientId",
            table: "PatientFamilyMembers",
            column: "LinkedPatientId");

        migrationBuilder.CreateIndex(
            name: "IX_PatientFamilyMembers_OwnerPatientId",
            table: "PatientFamilyMembers",
            column: "OwnerPatientId");

        migrationBuilder.CreateIndex(
            name: "IX_PatientFamilyMembers_OwnerPatientId_IsActive",
            table: "PatientFamilyMembers",
            columns: new[] { "OwnerPatientId", "IsActive" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "PatientFamilyMembers");
    }
}
