using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using RaphCare.Persistence;

#nullable disable

namespace RaphCare.Persistence.Migrations.BillingDb;

[DbContext(typeof(BillingDbContext))]
[Migration("20260902120000_AddStayInvoiceLines")]
public partial class AddStayInvoiceLines : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "AdmissionId",
            table: "Invoices",
            type: "uniqueidentifier",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "PaymentMethod",
            table: "Invoices",
            type: "nvarchar(50)",
            maxLength: 50,
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_Invoices_AdmissionId",
            table: "Invoices",
            column: "AdmissionId");

        migrationBuilder.CreateTable(
            name: "InvoiceLineItems",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                InvoiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ServiceType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                Quantity = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                UnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                TotalPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                ReferenceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_InvoiceLineItems", x => x.Id);
                table.ForeignKey(
                    name: "FK_InvoiceLineItems_Invoices_InvoiceId",
                    column: x => x.InvoiceId,
                    principalTable: "Invoices",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_InvoiceLineItems_InvoiceId",
            table: "InvoiceLineItems",
            column: "InvoiceId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "InvoiceLineItems");
        migrationBuilder.DropIndex(name: "IX_Invoices_AdmissionId", table: "Invoices");
        migrationBuilder.DropColumn(name: "AdmissionId", table: "Invoices");
        migrationBuilder.DropColumn(name: "PaymentMethod", table: "Invoices");
    }
}
