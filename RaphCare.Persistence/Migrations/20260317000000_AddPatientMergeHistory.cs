using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RaphCare.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPatientMergeHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PatientMergeHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PrimaryPatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MergedPatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MergedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MergedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientMergeHistory", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PatientMergeHistory_PrimaryPatientId",
                table: "PatientMergeHistory",
                column: "PrimaryPatientId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientMergeHistory_MergedPatientId",
                table: "PatientMergeHistory",
                column: "MergedPatientId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientMergeHistory_MergedAt",
                table: "PatientMergeHistory",
                column: "MergedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PatientMergeHistory");
        }
    }
}
