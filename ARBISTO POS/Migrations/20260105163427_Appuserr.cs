using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ARBISTO_POS.Migrations
{
    /// <inheritdoc />
    public partial class Appuserr : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CusImage",
                table: "AppUsers",
                newName: "UserImage");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserImage",
                table: "AppUsers",
                newName: "CusImage");
        }
    }
}
