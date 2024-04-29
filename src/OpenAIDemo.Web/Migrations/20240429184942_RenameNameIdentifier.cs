using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Haack.AIDemoWeb.Migrations
{
    /// <inheritdoc />
    public partial class RenameNameIdentifier : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Users",
                newName: "NameIdentifier");

            migrationBuilder.RenameIndex(
                name: "IX_Users_Name",
                table: "Users",
                newName: "IX_Users_NameIdentifier");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "NameIdentifier",
                table: "Users",
                newName: "Name");

            migrationBuilder.RenameIndex(
                name: "IX_Users_NameIdentifier",
                table: "Users",
                newName: "IX_Users_Name");
        }
    }
}
