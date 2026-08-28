using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using RaphCare.Persistence;

#nullable disable

namespace RaphCare.Persistence.Migrations;

[DbContext(typeof(ClinicalDbContext))]
[Migration("20260621130100_AddPatientSupportMessages")]
public partial class AddPatientSupportMessages : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "PatientSupportMessages",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Subject = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                Message = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                PatientEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PatientSupportMessages", x => x.Id);
                table.ForeignKey(
                    name: "FK_PatientSupportMessages_Patients_PatientId",
                    column: x => x.PatientId,
                    principalTable: "Patients",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_PatientSupportMessages_PatientId",
            table: "PatientSupportMessages",
            column: "PatientId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "PatientSupportMessages");
    }
}
