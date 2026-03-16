using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RaphCare.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MPI_UniqueIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Patients_NationalHealthId",
                table: "Patients");

            migrationBuilder.CreateIndex(
                name: "IX_Patients_NationalHealthId",
                table: "Patients",
                column: "NationalHealthId",
                unique: true,
                filter: "[NationalHealthId] IS NOT NULL");

            migrationBuilder.DropIndex(
                name: "IX_PatientExternalIds_SourceSystem_ExternalId",
                table: "PatientExternalIds");

            migrationBuilder.CreateIndex(
                name: "IX_PatientExternalIds_SourceSystem_ExternalId",
                table: "PatientExternalIds",
                columns: new[] { "SourceSystem", "ExternalId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Patients_NationalHealthId",
                table: "Patients");

            migrationBuilder.CreateIndex(
                name: "IX_Patients_NationalHealthId",
                table: "Patients",
                column: "NationalHealthId");

            migrationBuilder.DropIndex(
                name: "IX_PatientExternalIds_SourceSystem_ExternalId",
                table: "PatientExternalIds");

            migrationBuilder.CreateIndex(
                name: "IX_PatientExternalIds_SourceSystem_ExternalId",
                table: "PatientExternalIds",
                columns: new[] { "SourceSystem", "ExternalId" });
        }
    }
}
