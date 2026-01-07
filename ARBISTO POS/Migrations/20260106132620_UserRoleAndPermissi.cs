using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ARBISTO_POS.Migrations
{
    /// <inheritdoc />
    public partial class UserRoleAndPermissi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppUsers_UserRoles_RolesId",
                table: "AppUsers");

            migrationBuilder.DropIndex(
                name: "IX_AppUsers_RolesId",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "RolesId",
                table: "AppUsers");

            migrationBuilder.AlterColumn<int>(
                name: "RoleId",
                table: "AppUsers",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppUsers_RoleId",
                table: "AppUsers",
                column: "RoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_AppUsers_UserRoles_RoleId",
                table: "AppUsers",
                column: "RoleId",
                principalTable: "UserRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppUsers_UserRoles_RoleId",
                table: "AppUsers");

            migrationBuilder.DropIndex(
                name: "IX_AppUsers_RoleId",
                table: "AppUsers");

            migrationBuilder.AlterColumn<int>(
                name: "RoleId",
                table: "AppUsers",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "RolesId",
                table: "AppUsers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppUsers_RolesId",
                table: "AppUsers",
                column: "RolesId");

            migrationBuilder.AddForeignKey(
                name: "FK_AppUsers_UserRoles_RolesId",
                table: "AppUsers",
                column: "RolesId",
                principalTable: "UserRoles",
                principalColumn: "Id");
        }
    }
}
