using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Haack.AIDemoWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddVectorColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:citext", ",,")
                .Annotation("Npgsql:PostgresExtension:vector", ",,")
                .OldAnnotation("Npgsql:PostgresExtension:citext", ",,");

            migrationBuilder.AlterColumn<byte[]>(
                name: "Embeddings",
                table: "UserFacts",
                type: "vector(1536 )",
                nullable: false,
                oldClrType: typeof(List<float>),
                oldType: "real[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:citext", ",,")
                .OldAnnotation("Npgsql:PostgresExtension:citext", ",,")
                .OldAnnotation("Npgsql:PostgresExtension:vector", ",,");

            migrationBuilder.AlterColumn<List<float>>(
                name: "Embeddings",
                table: "UserFacts",
                type: "real[]",
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "vector(1536 )");
        }
    }
}
