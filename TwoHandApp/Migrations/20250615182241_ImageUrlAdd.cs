using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TwoHandApp.Migrations
{
    /// <inheritdoc />
    public partial class ImageUrlAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "imageUrl",
                table: "Posts",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "imageUrl",
                table: "Posts");
        }
    }
}
