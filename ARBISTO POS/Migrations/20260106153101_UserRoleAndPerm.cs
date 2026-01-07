using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ARBISTO_POS.Migrations
{
    /// <inheritdoc />
    public partial class UserRoleAndPerm : Migration
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

            migrationBuilder.CreateIndex(
                name: "IX_AppUsers_RoleId",
                table: "AppUsers",
                column: "RoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_AppUsers_UserRoles_RoleId",
                table: "AppUsers",
                column: "RoleId",
                principalTable: "UserRoles",
                principalColumn: "Id");
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
