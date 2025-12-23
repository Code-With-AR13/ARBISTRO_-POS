using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ARBISTO_POS.Migrations
{
    /// <inheritdoc />
    public partial class Modiferis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Modifiers",
                columns: table => new
                {
                    ItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ModeName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ModeDiscription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModeImage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ItemCost = table.Column<int>(type: "int", nullable: false),
                    ItemPrice = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Modifiers", x => x.ItemId);
                });

            migrationBuilder.CreateTable(
                name: "ModifierIngredients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    IngredientId = table.Column<int>(type: "int", nullable: false),
                    ConsumptionQty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Cost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AvailableQty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ModifiersItemId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModifierIngredients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ModifierIngredients_Ingredients_IngredientId",
                        column: x => x.IngredientId,
                        principalTable: "Ingredients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ModifierIngredients_Modifiers_ModifiersItemId",
                        column: x => x.ModifiersItemId,
                        principalTable: "Modifiers",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ModifierIngredients_IngredientId",
                table: "ModifierIngredients",
                column: "IngredientId");

            migrationBuilder.CreateIndex(
                name: "IX_ModifierIngredients_ModifiersItemId",
                table: "ModifierIngredients",
                column: "ModifiersItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ModifierIngredients");

            migrationBuilder.DropTable(
                name: "Modifiers");
        }
    }
}
