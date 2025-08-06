using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TwoHandApp.Migrations
{
    /// <inheritdoc />
    public partial class AddUserAdRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ad_AspNetUsers_UserId1",
                table: "Ad");

            migrationBuilder.DropTable(
                name: "AdPhoto");

            migrationBuilder.DropTable(
                name: "Advertisements");

            migrationBuilder.DropTable(
                name: "PostPhotos");

            migrationBuilder.DropTable(
                name: "Posts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Ad",
                table: "Ad");

            migrationBuilder.DropIndex(
                name: "IX_Ad_UserId1",
                table: "Ad");

            migrationBuilder.DropColumn(
                name: "IsPremium",
                table: "Ad");

            migrationBuilder.DropColumn(
                name: "IsStoreAd",
                table: "Ad");

            migrationBuilder.DropColumn(
                name: "IsVip",
                table: "Ad");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Ad");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "Ad");

            migrationBuilder.RenameTable(
                name: "Ad",
                newName: "Ads");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Ads",
                type: "text",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Ads",
                table: "Ads",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Ads_UserId",
                table: "Ads",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ads_AspNetUsers_UserId",
                table: "Ads",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ads_AspNetUsers_UserId",
                table: "Ads");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Ads",
                table: "Ads");

            migrationBuilder.DropIndex(
                name: "IX_Ads_UserId",
                table: "Ads");

            migrationBuilder.RenameTable(
                name: "Ads",
                newName: "Ad");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "Ad",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<bool>(
                name: "IsPremium",
                table: "Ad",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsStoreAd",
                table: "Ad",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsVip",
                table: "Ad",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Ad",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "UserId1",
                table: "Ad",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Ad",
                table: "Ad",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "AdPhoto",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AdId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsMain = table.Column<bool>(type: "boolean", nullable: false),
                    Url = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdPhoto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdPhoto_Ad_AdId",
                        column: x => x.AdId,
                        principalTable: "Ad",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Advertisements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Image = table.Column<string>(type: "text", nullable: true),
                    IsPro = table.Column<bool>(type: "boolean", nullable: false),
                    IsVip = table.Column<bool>(type: "boolean", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    ProExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Title = table.Column<string>(type: "text", nullable: false),
                    VipExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Advertisements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Posts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId1 = table.Column<string>(type: "text", nullable: false),
                    AllowMessages = table.Column<bool>(type: "boolean", nullable: false),
                    City = table.Column<string>(type: "text", nullable: false),
                    ContactNumber = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    HasDelivery = table.Column<bool>(type: "boolean", nullable: false),
                    IsNew = table.Column<bool>(type: "boolean", nullable: false),
                    PostNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PosterType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ProductType = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ViewCount = table.Column<int>(type: "integer", nullable: false),
                    imageUrl = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Posts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Posts_AspNetUsers_UserId1",
                        column: x => x.UserId1,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PostPhotos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PostId = table.Column<int>(type: "integer", nullable: false),
                    Url = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostPhotos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PostPhotos_Posts_PostId",
                        column: x => x.PostId,
                        principalTable: "Posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Ad_UserId1",
                table: "Ad",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_AdPhoto_AdId",
                table: "AdPhoto",
                column: "AdId");

            migrationBuilder.CreateIndex(
                name: "IX_PostPhotos_PostId",
                table: "PostPhotos",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_UserId1",
                table: "Posts",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Ad_AspNetUsers_UserId1",
                table: "Ad",
                column: "UserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
