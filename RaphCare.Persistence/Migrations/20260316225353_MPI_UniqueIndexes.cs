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
            // Index may be missing if an earlier migration was skipped or the DB was created out of band.
            migrationBuilder.Sql(
                "DROP INDEX IF EXISTS [IX_Patients_NationalHealthId] ON [Patients];");

            migrationBuilder.CreateIndex(
                name: "IX_Patients_NationalHealthId",
                table: "Patients",
                column: "NationalHealthId",
                unique: true,
                filter: "[NationalHealthId] IS NOT NULL");

            migrationBuilder.Sql(
                "DROP INDEX IF EXISTS [IX_PatientExternalIds_SourceSystem_ExternalId] ON [PatientExternalIds];");

            migrationBuilder.CreateIndex(
                name: "IX_PatientExternalIds_SourceSystem_ExternalId",
                table: "PatientExternalIds",
                columns: new[] { "SourceSystem", "ExternalId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "DROP INDEX IF EXISTS [IX_Patients_NationalHealthId] ON [Patients];");

            migrationBuilder.CreateIndex(
                name: "IX_Patients_NationalHealthId",
                table: "Patients",
                column: "NationalHealthId");

            migrationBuilder.Sql(
                "DROP INDEX IF EXISTS [IX_PatientExternalIds_SourceSystem_ExternalId] ON [PatientExternalIds];");

            migrationBuilder.CreateIndex(
                name: "IX_PatientExternalIds_SourceSystem_ExternalId",
                table: "PatientExternalIds",
                columns: new[] { "SourceSystem", "ExternalId" });
        }
    }
}
