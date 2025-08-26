using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TwoHandApp.Migrations
{
    /// <inheritdoc />
    public partial class endpointsforlookup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AdImage_Ads_AdId",
                table: "AdImage");

            migrationBuilder.DropForeignKey(
                name: "FK_Ads_AdType_AdTypeId",
                table: "Ads");

            migrationBuilder.DropForeignKey(
                name: "FK_Ads_Category_CategoryId",
                table: "Ads");

            migrationBuilder.DropForeignKey(
                name: "FK_Ads_City_CityId",
                table: "Ads");

            migrationBuilder.DropPrimaryKey(
                name: "PK_City",
                table: "City");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Category",
                table: "Category");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AdType",
                table: "AdType");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AdImage",
                table: "AdImage");

            migrationBuilder.RenameTable(
                name: "City",
                newName: "Cities");

            migrationBuilder.RenameTable(
                name: "Category",
                newName: "Categories");

            migrationBuilder.RenameTable(
                name: "AdType",
                newName: "AdTypes");

            migrationBuilder.RenameTable(
                name: "AdImage",
                newName: "AdImages");

            migrationBuilder.RenameIndex(
                name: "IX_AdImage_AdId",
                table: "AdImages",
                newName: "IX_AdImages_AdId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Cities",
                table: "Cities",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Categories",
                table: "Categories",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AdTypes",
                table: "AdTypes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AdImages",
                table: "AdImages",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AdImages_Ads_AdId",
                table: "AdImages",
                column: "AdId",
                principalTable: "Ads",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Ads_AdTypes_AdTypeId",
                table: "Ads",
                column: "AdTypeId",
                principalTable: "AdTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Ads_Categories_CategoryId",
                table: "Ads",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Ads_Cities_CityId",
                table: "Ads",
                column: "CityId",
                principalTable: "Cities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AdImages_Ads_AdId",
                table: "AdImages");

            migrationBuilder.DropForeignKey(
                name: "FK_Ads_AdTypes_AdTypeId",
                table: "Ads");

            migrationBuilder.DropForeignKey(
                name: "FK_Ads_Categories_CategoryId",
                table: "Ads");

            migrationBuilder.DropForeignKey(
                name: "FK_Ads_Cities_CityId",
                table: "Ads");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Cities",
                table: "Cities");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Categories",
                table: "Categories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AdTypes",
                table: "AdTypes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AdImages",
                table: "AdImages");

            migrationBuilder.RenameTable(
                name: "Cities",
                newName: "City");

            migrationBuilder.RenameTable(
                name: "Categories",
                newName: "Category");

            migrationBuilder.RenameTable(
                name: "AdTypes",
                newName: "AdType");

            migrationBuilder.RenameTable(
                name: "AdImages",
                newName: "AdImage");

            migrationBuilder.RenameIndex(
                name: "IX_AdImages_AdId",
                table: "AdImage",
                newName: "IX_AdImage_AdId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_City",
                table: "City",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Category",
                table: "Category",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AdType",
                table: "AdType",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AdImage",
                table: "AdImage",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AdImage_Ads_AdId",
                table: "AdImage",
                column: "AdId",
                principalTable: "Ads",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Ads_AdType_AdTypeId",
                table: "Ads",
                column: "AdTypeId",
                principalTable: "AdType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Ads_Category_CategoryId",
                table: "Ads",
                column: "CategoryId",
                principalTable: "Category",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Ads_City_CityId",
                table: "Ads",
                column: "CityId",
                principalTable: "City",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
