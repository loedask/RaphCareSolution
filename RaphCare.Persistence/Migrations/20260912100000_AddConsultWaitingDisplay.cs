using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using RaphCare.Persistence;

#nullable disable

namespace RaphCare.Persistence.Migrations;

[DbContext(typeof(ClinicalDbContext))]
[Migration("20260912100000_AddConsultWaitingDisplay")]
public partial class AddConsultWaitingDisplay : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "ConsultDisplayToken",
            table: "Clinics",
            type: "nvarchar(12)",
            maxLength: 12,
            nullable: true);

        migrationBuilder.CreateTable(
            name: "ConsultTickets",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ClinicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                AppointmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                ProviderId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                QueueCode = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: false),
                Status = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                ArrivedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                CalledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                CreatedByApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ConsultTickets", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Clinics_ConsultDisplayToken",
            table: "Clinics",
            column: "ConsultDisplayToken",
            unique: true,
            filter: "[ConsultDisplayToken] IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_ConsultTickets_AppointmentId",
            table: "ConsultTickets",
            column: "AppointmentId");

        migrationBuilder.CreateIndex(
            name: "IX_ConsultTickets_CalledAt",
            table: "ConsultTickets",
            column: "CalledAt");

        migrationBuilder.CreateIndex(
            name: "IX_ConsultTickets_ClinicId",
            table: "ConsultTickets",
            column: "ClinicId");

        migrationBuilder.CreateIndex(
            name: "IX_ConsultTickets_ClinicId_Status",
            table: "ConsultTickets",
            columns: new[] { "ClinicId", "Status" });

        migrationBuilder.CreateIndex(
            name: "IX_ConsultTickets_QueueCode",
            table: "ConsultTickets",
            column: "QueueCode");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "ConsultTickets");
        migrationBuilder.DropIndex(name: "IX_Clinics_ConsultDisplayToken", table: "Clinics");
        migrationBuilder.DropColumn(name: "ConsultDisplayToken", table: "Clinics");
    }
}
