using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShortURL.Migrations
{
    /// <inheritdoc />
    public partial class AddMetadataFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClickCount",
                table: "ShortenedUrls",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastAccessedAt",
                table: "ShortenedUrls",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClickCount",
                table: "ShortenedUrls");

            migrationBuilder.DropColumn(
                name: "LastAccessedAt",
                table: "ShortenedUrls");
        }
    }
}
