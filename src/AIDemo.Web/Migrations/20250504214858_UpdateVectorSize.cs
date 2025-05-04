using Microsoft.EntityFrameworkCore.Migrations;
using Pgvector;

#nullable disable

namespace Haack.AIDemoWeb.Migrations
{
    /// <inheritdoc />
    public partial class UpdateVectorSize : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Vector>(
                name: "Embeddings",
                table: "ContactFacts",
                type: "vector(3072)",
                nullable: false,
                oldClrType: typeof(Vector),
                oldType: "vector(1536)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Vector>(
                name: "Embeddings",
                table: "ContactFacts",
                type: "vector(1536)",
                nullable: false,
                oldClrType: typeof(Vector),
                oldType: "vector(3072)");
        }
    }
}
