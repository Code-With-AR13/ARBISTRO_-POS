using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ARBISTO_POS.Migrations
{
    /// <inheritdoc />
    public partial class PickPoints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "Modifiers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "Items",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "Ingredients",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "FoodCategories",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "Customers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "PickPoints",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PicTittle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PicDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PersonName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhoneNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PickPoints", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PickPoints");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "Modifiers");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "Ingredients");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "FoodCategories");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "Customers");
        }
    }
}
