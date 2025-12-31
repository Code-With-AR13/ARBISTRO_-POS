using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ARBISTO_POS.Migrations
{
    /// <inheritdoc />
    public partial class Heldordersa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HeldOrdersOrderId",
                table: "SaleOrderItems",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "HeldOrderItems",
                columns: table => new
                {
                    OrderItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: true),
                    CustomerId = table.Column<int>(type: "int", nullable: true),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    ItemName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: true),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HeldOrderItems", x => x.OrderItemId);
                    table.ForeignKey(
                        name: "FK_HeldOrderItems_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_HeldOrderItems_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HeldOrderItems_SaleOrders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "SaleOrders",
                        principalColumn: "OrderId");
                });

            migrationBuilder.CreateTable(
                name: "HeldOrders",
                columns: table => new
                {
                    OrderId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrderDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OrderType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrderStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomerId = table.Column<int>(type: "int", nullable: true),
                    TableId = table.Column<int>(type: "int", nullable: true),
                    PickUpId = table.Column<int>(type: "int", nullable: true),
                    DelivaryAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaymentId = table.Column<int>(type: "int", nullable: true),
                    PaymentStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SubTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    GrandTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ChefId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HeldOrders", x => x.OrderId);
                    table.ForeignKey(
                        name: "FK_HeldOrders_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_HeldOrders_Employees_ChefId",
                        column: x => x.ChefId,
                        principalTable: "Employees",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_HeldOrders_PaymentMethods_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "PaymentMethods",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_HeldOrders_PickPoints_PickUpId",
                        column: x => x.PickUpId,
                        principalTable: "PickPoints",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_HeldOrders_ServiceTables_TableId",
                        column: x => x.TableId,
                        principalTable: "ServiceTables",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_SaleOrderItems_HeldOrdersOrderId",
                table: "SaleOrderItems",
                column: "HeldOrdersOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_HeldOrderItems_CustomerId",
                table: "HeldOrderItems",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_HeldOrderItems_ItemId",
                table: "HeldOrderItems",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_HeldOrderItems_OrderId",
                table: "HeldOrderItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_HeldOrders_ChefId",
                table: "HeldOrders",
                column: "ChefId");

            migrationBuilder.CreateIndex(
                name: "IX_HeldOrders_CustomerId",
                table: "HeldOrders",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_HeldOrders_PaymentId",
                table: "HeldOrders",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_HeldOrders_PickUpId",
                table: "HeldOrders",
                column: "PickUpId");

            migrationBuilder.CreateIndex(
                name: "IX_HeldOrders_TableId",
                table: "HeldOrders",
                column: "TableId");

            migrationBuilder.AddForeignKey(
                name: "FK_SaleOrderItems_HeldOrders_HeldOrdersOrderId",
                table: "SaleOrderItems",
                column: "HeldOrdersOrderId",
                principalTable: "HeldOrders",
                principalColumn: "OrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SaleOrderItems_HeldOrders_HeldOrdersOrderId",
                table: "SaleOrderItems");

            migrationBuilder.DropTable(
                name: "HeldOrderItems");

            migrationBuilder.DropTable(
                name: "HeldOrders");

            migrationBuilder.DropIndex(
                name: "IX_SaleOrderItems_HeldOrdersOrderId",
                table: "SaleOrderItems");

            migrationBuilder.DropColumn(
                name: "HeldOrdersOrderId",
                table: "SaleOrderItems");
        }
    }
}
