using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using RaphCare.Persistence;

#nullable disable

namespace RaphCare.Persistence.Migrations;

[DbContext(typeof(ClinicalDbContext))]
[Migration("20260902210000_AddOutboundReferralTracking")]
public partial class AddOutboundReferralTracking : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Referrals",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ClinicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                VisitId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                ReferredTo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                Specialty = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                Status = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                ReferredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                AcceptedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                CreatedByApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Referrals", x => x.Id);
                table.ForeignKey(
                    name: "FK_Referrals_Visits_VisitId",
                    column: x => x.VisitId,
                    principalTable: "Visits",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Referrals_ClinicId",
            table: "Referrals",
            column: "ClinicId");

        migrationBuilder.CreateIndex(
            name: "IX_Referrals_ClinicId_Status",
            table: "Referrals",
            columns: new[] { "ClinicId", "Status" });

        migrationBuilder.CreateIndex(
            name: "IX_Referrals_PatientId",
            table: "Referrals",
            column: "PatientId");

        migrationBuilder.CreateIndex(
            name: "IX_Referrals_VisitId",
            table: "Referrals",
            column: "VisitId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Referrals");
    }
}
