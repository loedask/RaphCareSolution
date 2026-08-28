using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using RaphCare.Persistence;

#nullable disable

namespace RaphCare.Persistence.Migrations;

[DbContext(typeof(ClinicalDbContext))]
[Migration("20260828223000_AddCollectionOrderFields")]
public partial class AddCollectionOrderFields : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Status",
            table: "Prescription",
            type: "nvarchar(32)",
            maxLength: 32,
            nullable: false,
            defaultValue: "Pending");

        migrationBuilder.AddColumn<string>(
            name: "PickupCode",
            table: "Prescription",
            type: "nvarchar(8)",
            maxLength: 8,
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<DateTime>(
            name: "DispensedAt",
            table: "Prescription",
            type: "datetime2",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "Status",
            table: "LabRequest",
            type: "nvarchar(32)",
            maxLength: 32,
            nullable: false,
            defaultValue: "Pending");

        migrationBuilder.AddColumn<string>(
            name: "PickupCode",
            table: "LabRequest",
            type: "nvarchar(8)",
            maxLength: 8,
            nullable: false,
            defaultValue: "");

        migrationBuilder.Sql(
            """
            UPDATE lr
            SET Status = N'Completed'
            FROM LabRequest lr
            WHERE EXISTS (SELECT 1 FROM LabResult r WHERE r.LabRequestId = lr.Id);

            DECLARE @alpha nvarchar(32) = N'ABCDEFGHJKLMNPQRSTUVWXYZ23456789';

            ;WITH rx AS (
                SELECT Id, ROW_NUMBER() OVER (ORDER BY CreatedAt, Id) AS Seq
                FROM Prescription
            )
            UPDATE p
            SET PickupCode =
                SUBSTRING(@alpha, ((rx.Seq - 1) / 33554432) % 32 + 1, 1)
                + SUBSTRING(@alpha, ((rx.Seq - 1) / 1048576) % 32 + 1, 1)
                + SUBSTRING(@alpha, ((rx.Seq - 1) / 32768) % 32 + 1, 1)
                + SUBSTRING(@alpha, ((rx.Seq - 1) / 1024) % 32 + 1, 1)
                + SUBSTRING(@alpha, ((rx.Seq - 1) / 32) % 32 + 1, 1)
                + SUBSTRING(@alpha, (rx.Seq - 1) % 32 + 1, 1)
            FROM Prescription p
            INNER JOIN rx ON rx.Id = p.Id;

            ;WITH labs AS (
                SELECT Id, ROW_NUMBER() OVER (ORDER BY CreatedAt, Id) AS Seq
                FROM LabRequest
            )
            UPDATE l
            SET PickupCode =
                SUBSTRING(@alpha, ((labs.Seq - 1) / 33554432) % 32 + 1, 1)
                + SUBSTRING(@alpha, ((labs.Seq - 1) / 1048576) % 32 + 1, 1)
                + SUBSTRING(@alpha, ((labs.Seq - 1) / 32768) % 32 + 1, 1)
                + SUBSTRING(@alpha, ((labs.Seq - 1) / 1024) % 32 + 1, 1)
                + SUBSTRING(@alpha, ((labs.Seq - 1) / 32) % 32 + 1, 1)
                + SUBSTRING(@alpha, (labs.Seq - 1) % 32 + 1, 1)
            FROM LabRequest l
            INNER JOIN labs ON labs.Id = l.Id;
            """);

        migrationBuilder.CreateIndex(
            name: "IX_Prescription_PickupCode",
            table: "Prescription",
            column: "PickupCode",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Prescription_Status",
            table: "Prescription",
            column: "Status");

        migrationBuilder.CreateIndex(
            name: "IX_LabRequest_PickupCode",
            table: "LabRequest",
            column: "PickupCode",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_LabRequest_Status",
            table: "LabRequest",
            column: "Status");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "IX_Prescription_PickupCode", table: "Prescription");
        migrationBuilder.DropIndex(name: "IX_Prescription_Status", table: "Prescription");
        migrationBuilder.DropIndex(name: "IX_LabRequest_PickupCode", table: "LabRequest");
        migrationBuilder.DropIndex(name: "IX_LabRequest_Status", table: "LabRequest");

        migrationBuilder.DropColumn(name: "DispensedAt", table: "Prescription");
        migrationBuilder.DropColumn(name: "PickupCode", table: "Prescription");
        migrationBuilder.DropColumn(name: "Status", table: "Prescription");
        migrationBuilder.DropColumn(name: "PickupCode", table: "LabRequest");
        migrationBuilder.DropColumn(name: "Status", table: "LabRequest");
    }
}
