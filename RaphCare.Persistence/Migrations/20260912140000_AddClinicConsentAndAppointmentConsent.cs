using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using RaphCare.Persistence;

#nullable disable

namespace RaphCare.Persistence.Migrations;

[DbContext(typeof(ClinicalDbContext))]
[Migration("20260912140000_AddClinicConsentAndAppointmentConsent")]
public partial class AddClinicConsentAndAppointmentConsent : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ClinicConsentTemplates",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ClinicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                Body = table.Column<string>(type: "nvarchar(8000)", maxLength: 8000, nullable: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ClinicConsentTemplates", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "AppointmentConsents",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                AppointmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ClinicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                TemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                TitleSnapshot = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                BodySnapshot = table.Column<string>(type: "nvarchar(8000)", maxLength: 8000, nullable: false),
                SignedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                SignedByApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AppointmentConsents", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_ClinicConsentTemplates_ClinicId",
            table: "ClinicConsentTemplates",
            column: "ClinicId");

        migrationBuilder.CreateIndex(
            name: "IX_ClinicConsentTemplates_ClinicId_IsActive",
            table: "ClinicConsentTemplates",
            columns: new[] { "ClinicId", "IsActive" });

        migrationBuilder.CreateIndex(
            name: "IX_AppointmentConsents_AppointmentId",
            table: "AppointmentConsents",
            column: "AppointmentId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_AppointmentConsents_PatientId",
            table: "AppointmentConsents",
            column: "PatientId");

        migrationBuilder.CreateIndex(
            name: "IX_AppointmentConsents_ClinicId",
            table: "AppointmentConsents",
            column: "ClinicId");

        migrationBuilder.CreateIndex(
            name: "IX_AppointmentConsents_TemplateId",
            table: "AppointmentConsents",
            column: "TemplateId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "AppointmentConsents");
        migrationBuilder.DropTable(name: "ClinicConsentTemplates");
    }
}
