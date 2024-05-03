using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Haack.AIDemoWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddBirthday : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Birthday_Day",
                table: "Contacts",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Birthday_Month",
                table: "Contacts",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Birthday_Year",
                table: "Contacts",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Birthday_Day",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "Birthday_Month",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "Birthday_Year",
                table: "Contacts");
        }
    }
}
