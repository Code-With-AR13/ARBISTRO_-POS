using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ARBISTO_POS.Migrations
{
    /// <inheritdoc />
    public partial class submit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SaleOrders_ServiceTables_TableId",
                table: "SaleOrders");

            migrationBuilder.AlterColumn<int>(
                name: "TableId",
                table: "SaleOrders",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_SaleOrders_ServiceTables_TableId",
                table: "SaleOrders",
                column: "TableId",
                principalTable: "ServiceTables",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SaleOrders_ServiceTables_TableId",
                table: "SaleOrders");

            migrationBuilder.AlterColumn<int>(
                name: "TableId",
                table: "SaleOrders",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SaleOrders_ServiceTables_TableId",
                table: "SaleOrders",
                column: "TableId",
                principalTable: "ServiceTables",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
