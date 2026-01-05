using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ARBISTO_POS.Migrations
{
    /// <inheritdoc />
    public partial class Forgetpassward : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PasswordResetToken",
                table: "AppUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TokenExpiry",
                table: "AppUsers",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PasswordResetToken",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "TokenExpiry",
                table: "AppUsers");
        }
    }
}
