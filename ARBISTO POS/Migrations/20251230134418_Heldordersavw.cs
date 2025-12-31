using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ARBISTO_POS.Migrations
{
    /// <inheritdoc />
    public partial class Heldordersavw : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HeldOrderItems_SaleOrders_OrderId",
                table: "HeldOrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_SaleOrderItems_HeldOrders_HeldOrdersOrderId",
                table: "SaleOrderItems");

            migrationBuilder.DropIndex(
                name: "IX_SaleOrderItems_HeldOrdersOrderId",
                table: "SaleOrderItems");

            migrationBuilder.DropColumn(
                name: "HeldOrdersOrderId",
                table: "SaleOrderItems");

            migrationBuilder.AddForeignKey(
                name: "FK_HeldOrderItems_HeldOrders_OrderId",
                table: "HeldOrderItems",
                column: "OrderId",
                principalTable: "HeldOrders",
                principalColumn: "OrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HeldOrderItems_HeldOrders_OrderId",
                table: "HeldOrderItems");

            migrationBuilder.AddColumn<int>(
                name: "HeldOrdersOrderId",
                table: "SaleOrderItems",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SaleOrderItems_HeldOrdersOrderId",
                table: "SaleOrderItems",
                column: "HeldOrdersOrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_HeldOrderItems_SaleOrders_OrderId",
                table: "HeldOrderItems",
                column: "OrderId",
                principalTable: "SaleOrders",
                principalColumn: "OrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_SaleOrderItems_HeldOrders_HeldOrdersOrderId",
                table: "SaleOrderItems",
                column: "HeldOrdersOrderId",
                principalTable: "HeldOrders",
                principalColumn: "OrderId");
        }
    }
}
