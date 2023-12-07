using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Haack.AIDemoWeb.Migrations
{
    /// <inheritdoc />
    public partial class ThreadAssistantId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AssistantId",
                table: "Threads",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssistantId",
                table: "Threads");
        }
    }
}
