using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ARBISTO_POS.Migrations
{
    /// <inheritdoc />
    public partial class Paymentop : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SaleOrders_PaymentMethods_PaymentId",
                table: "SaleOrders");

            migrationBuilder.AlterColumn<int>(
                name: "PaymentId",
                table: "SaleOrders",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_SaleOrders_PaymentMethods_PaymentId",
                table: "SaleOrders",
                column: "PaymentId",
                principalTable: "PaymentMethods",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SaleOrders_PaymentMethods_PaymentId",
                table: "SaleOrders");

            migrationBuilder.AlterColumn<int>(
                name: "PaymentId",
                table: "SaleOrders",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SaleOrders_PaymentMethods_PaymentId",
                table: "SaleOrders",
                column: "PaymentId",
                principalTable: "PaymentMethods",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
