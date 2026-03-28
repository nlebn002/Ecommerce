using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ecommerce.BasketService.Infrastructure.Migrations;

public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Baskets",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                CustomerId = table.Column<string>(type: "text", nullable: false),
                Status = table.Column<string>(type: "text", nullable: false),
                Total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                ConcurrencyToken = table.Column<Guid>(type: "uuid", nullable: false),
                CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Baskets", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "BasketItems",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                BasketId = table.Column<Guid>(type: "uuid", nullable: false),
                ProductId = table.Column<string>(type: "text", nullable: false),
                ProductName = table.Column<string>(type: "text", nullable: false),
                Quantity = table.Column<int>(type: "integer", nullable: false),
                UnitPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                ConcurrencyToken = table.Column<Guid>(type: "uuid", nullable: false),
                CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_BasketItems", x => x.Id);
                table.ForeignKey(
                    name: "FK_BasketItems_Baskets_BasketId",
                    column: x => x.BasketId,
                    principalTable: "Baskets",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_BasketItems_BasketId",
            table: "BasketItems",
            column: "BasketId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "BasketItems");

        migrationBuilder.DropTable(
            name: "Baskets");
    }
}
