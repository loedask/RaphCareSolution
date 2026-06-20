using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RaphCare.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddClinicStaffMembership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RegisteredByApplicationUserId",
                table: "Clinics",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ClinicStaffMemberships",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClinicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicStaffMemberships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClinicStaffMemberships_Clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "Clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClinicStaffMemberships_ApplicationUserId_ClinicId",
                table: "ClinicStaffMemberships",
                columns: new[] { "ApplicationUserId", "ClinicId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClinicStaffMemberships_ClinicId",
                table: "ClinicStaffMemberships",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicStaffMemberships_IsActive",
                table: "ClinicStaffMemberships",
                column: "IsActive");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClinicStaffMemberships");

            migrationBuilder.DropColumn(
                name: "RegisteredByApplicationUserId",
                table: "Clinics");
        }
    }
}
