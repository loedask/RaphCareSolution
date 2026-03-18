using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RaphCare.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSpecialtyManyToMany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Specialty",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Specialty", x => x.Id);
                });

            // Preserve existing provider specialties by migrating Provider.Specialty into the new lookup + join table.
            // This migration assumes Provider.Specialty existed before this change.
            migrationBuilder.Sql(@"
INSERT INTO Specialty (Id, Name, CreatedAt, UpdatedAt)
SELECT NEWID(),
       LTRIM(RTRIM(t.Specialty)) AS Name,
       SYSUTCDATETIME()          AS CreatedAt,
       NULL                       AS UpdatedAt
FROM (
    SELECT DISTINCT Specialty
    FROM Provider
) t
WHERE t.Specialty IS NOT NULL
  AND LTRIM(RTRIM(t.Specialty)) <> '';
");

            migrationBuilder.CreateTable(
                name: "Therapist",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClinicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Certification = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Therapist", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Therapist_Clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "Clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProviderSpecialty",
                columns: table => new
                {
                    ProvidersId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SpecialtiesId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderSpecialty", x => new { x.ProvidersId, x.SpecialtiesId });
                    table.ForeignKey(
                        name: "FK_ProviderSpecialty_Provider_ProvidersId",
                        column: x => x.ProvidersId,
                        principalTable: "Provider",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProviderSpecialty_Specialty_SpecialtiesId",
                        column: x => x.SpecialtiesId,
                        principalTable: "Specialty",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SpecialtyTherapist",
                columns: table => new
                {
                    SpecialtiesId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TherapistsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpecialtyTherapist", x => new { x.SpecialtiesId, x.TherapistsId });
                    table.ForeignKey(
                        name: "FK_SpecialtyTherapist_Specialty_SpecialtiesId",
                        column: x => x.SpecialtiesId,
                        principalTable: "Specialty",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SpecialtyTherapist_Therapist_TherapistsId",
                        column: x => x.TherapistsId,
                        principalTable: "Therapist",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.Sql(@"
INSERT INTO ProviderSpecialty (ProvidersId, SpecialtiesId)
SELECT p.Id,
       s.Id
FROM Provider p
INNER JOIN Specialty s
    ON s.Name = LTRIM(RTRIM(p.Specialty));
");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderSpecialty_SpecialtiesId",
                table: "ProviderSpecialty",
                column: "SpecialtiesId");

            migrationBuilder.CreateIndex(
                name: "IX_SpecialtyTherapist_TherapistsId",
                table: "SpecialtyTherapist",
                column: "TherapistsId");

            migrationBuilder.CreateIndex(
                name: "IX_Therapist_ClinicId",
                table: "Therapist",
                column: "ClinicId");

            migrationBuilder.DropColumn(
                name: "Specialty",
                table: "Provider");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProviderSpecialty");

            migrationBuilder.DropTable(
                name: "SpecialtyTherapist");

            migrationBuilder.DropTable(
                name: "Specialty");

            migrationBuilder.DropTable(
                name: "Therapist");

            migrationBuilder.AddColumn<string>(
                name: "Specialty",
                table: "Provider",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");
        }
    }
}
