using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TwoHandApp.Migrations
{
    /// <inheritdoc />
    public partial class IsStore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsStore",
                table: "Ads",
                type: "boolean",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsStore",
                table: "Ads");
        }
    }
}
