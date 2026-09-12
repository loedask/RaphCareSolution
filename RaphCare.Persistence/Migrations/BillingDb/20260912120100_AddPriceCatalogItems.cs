using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using RaphCare.Persistence;

#nullable disable

namespace RaphCare.Persistence.Migrations.BillingDb;

[DbContext(typeof(BillingDbContext))]
[Migration("20260912120100_AddPriceCatalogItems")]
public partial class AddPriceCatalogItems : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "PriceCatalogItems",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                SkuCode = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                DisplayName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                Category = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                AmountZar = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                AmountUsd = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PriceCatalogItems", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_PriceCatalogItems_Category",
            table: "PriceCatalogItems",
            column: "Category");

        migrationBuilder.CreateIndex(
            name: "IX_PriceCatalogItems_IsActive",
            table: "PriceCatalogItems",
            column: "IsActive");

        migrationBuilder.CreateIndex(
            name: "IX_PriceCatalogItems_SkuCode",
            table: "PriceCatalogItems",
            column: "SkuCode",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "PriceCatalogItems");
    }
}
