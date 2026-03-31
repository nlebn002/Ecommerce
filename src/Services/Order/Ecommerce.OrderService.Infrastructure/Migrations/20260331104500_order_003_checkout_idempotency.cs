using System;
using Ecommerce.OrderService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecommerce.OrderService.Infrastructure.Migrations
{
    [DbContext(typeof(OrderDbContext))]
    [Migration("20260331104500_order_003_checkout_idempotency")]
    public partial class order_003_checkout_idempotency : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CheckoutCorrelationId",
                table: "Orders",
                type: "uuid",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE "Orders"
                SET "CheckoutCorrelationId" = "Id"
                WHERE "CheckoutCorrelationId" IS NULL;
                """);

            migrationBuilder.AlterColumn<Guid>(
                name: "CheckoutCorrelationId",
                table: "Orders",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CheckoutCorrelationId",
                table: "Orders",
                column: "CheckoutCorrelationId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Orders_CheckoutCorrelationId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CheckoutCorrelationId",
                table: "Orders");
        }
    }
}
