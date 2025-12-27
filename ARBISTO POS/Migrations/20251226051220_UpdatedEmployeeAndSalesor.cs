using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ARBISTO_POS.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedEmployeeAndSalesor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SaleOrderItems_SaleOrders_OrdersOrderId",
                table: "SaleOrderItems");

            migrationBuilder.DropIndex(
                name: "IX_SaleOrderItems_OrdersOrderId",
                table: "SaleOrderItems");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "SaleOrders");

            migrationBuilder.DropColumn(
                name: "CustomerName",
                table: "SaleOrders");

            migrationBuilder.DropColumn(
                name: "CustomerPhone",
                table: "SaleOrders");

            migrationBuilder.DropColumn(
                name: "DeliveryAddress",
                table: "SaleOrders");

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "SaleOrders");

            migrationBuilder.DropColumn(
                name: "CusName",
                table: "SaleOrderItems");

            migrationBuilder.DropColumn(
                name: "OrdersOrderId",
                table: "SaleOrderItems");

            migrationBuilder.RenameColumn(
                name: "ShiefId",
                table: "SaleOrders",
                newName: "PickUpId");

            migrationBuilder.AddColumn<int>(
                name: "ChefId",
                table: "SaleOrders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CustomerId",
                table: "SaleOrders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PaymentId",
                table: "SaleOrders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CustomerId",
                table: "SaleOrderItems",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SaleOrders_ChefId",
                table: "SaleOrders",
                column: "ChefId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleOrders_CustomerId",
                table: "SaleOrders",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleOrders_PaymentId",
                table: "SaleOrders",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleOrders_PickUpId",
                table: "SaleOrders",
                column: "PickUpId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleOrders_TableId",
                table: "SaleOrders",
                column: "TableId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleOrderItems_CustomerId",
                table: "SaleOrderItems",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleOrderItems_ItemId",
                table: "SaleOrderItems",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleOrderItems_OrderId",
                table: "SaleOrderItems",
                column: "OrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_SaleOrderItems_Customers_CustomerId",
                table: "SaleOrderItems",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SaleOrderItems_Items_ItemId",
                table: "SaleOrderItems",
                column: "ItemId",
                principalTable: "Items",
                principalColumn: "ItemId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SaleOrderItems_SaleOrders_OrderId",
                table: "SaleOrderItems",
                column: "OrderId",
                principalTable: "SaleOrders",
                principalColumn: "OrderId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SaleOrders_Customers_CustomerId",
                table: "SaleOrders",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SaleOrders_Employees_ChefId",
                table: "SaleOrders",
                column: "ChefId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SaleOrders_PaymentMethods_PaymentId",
                table: "SaleOrders",
                column: "PaymentId",
                principalTable: "PaymentMethods",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SaleOrders_PickPoints_PickUpId",
                table: "SaleOrders",
                column: "PickUpId",
                principalTable: "PickPoints",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SaleOrders_ServiceTables_TableId",
                table: "SaleOrders",
                column: "TableId",
                principalTable: "ServiceTables",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SaleOrderItems_Customers_CustomerId",
                table: "SaleOrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_SaleOrderItems_Items_ItemId",
                table: "SaleOrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_SaleOrderItems_SaleOrders_OrderId",
                table: "SaleOrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_SaleOrders_Customers_CustomerId",
                table: "SaleOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_SaleOrders_Employees_ChefId",
                table: "SaleOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_SaleOrders_PaymentMethods_PaymentId",
                table: "SaleOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_SaleOrders_PickPoints_PickUpId",
                table: "SaleOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_SaleOrders_ServiceTables_TableId",
                table: "SaleOrders");

            migrationBuilder.DropIndex(
                name: "IX_SaleOrders_ChefId",
                table: "SaleOrders");

            migrationBuilder.DropIndex(
                name: "IX_SaleOrders_CustomerId",
                table: "SaleOrders");

            migrationBuilder.DropIndex(
                name: "IX_SaleOrders_PaymentId",
                table: "SaleOrders");

            migrationBuilder.DropIndex(
                name: "IX_SaleOrders_PickUpId",
                table: "SaleOrders");

            migrationBuilder.DropIndex(
                name: "IX_SaleOrders_TableId",
                table: "SaleOrders");

            migrationBuilder.DropIndex(
                name: "IX_SaleOrderItems_CustomerId",
                table: "SaleOrderItems");

            migrationBuilder.DropIndex(
                name: "IX_SaleOrderItems_ItemId",
                table: "SaleOrderItems");

            migrationBuilder.DropIndex(
                name: "IX_SaleOrderItems_OrderId",
                table: "SaleOrderItems");

            migrationBuilder.DropColumn(
                name: "ChefId",
                table: "SaleOrders");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "SaleOrders");

            migrationBuilder.DropColumn(
                name: "PaymentId",
                table: "SaleOrders");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "SaleOrderItems");

            migrationBuilder.RenameColumn(
                name: "PickUpId",
                table: "SaleOrders",
                newName: "ShiefId");

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "SaleOrders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerName",
                table: "SaleOrders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CustomerPhone",
                table: "SaleOrders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DeliveryAddress",
                table: "SaleOrders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PaymentMethod",
                table: "SaleOrders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CusName",
                table: "SaleOrderItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "OrdersOrderId",
                table: "SaleOrderItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_SaleOrderItems_OrdersOrderId",
                table: "SaleOrderItems",
                column: "OrdersOrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_SaleOrderItems_SaleOrders_OrdersOrderId",
                table: "SaleOrderItems",
                column: "OrdersOrderId",
                principalTable: "SaleOrders",
                principalColumn: "OrderId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
