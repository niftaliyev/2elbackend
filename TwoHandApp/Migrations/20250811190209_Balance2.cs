using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TwoHandApp.Migrations
{
    /// <inheritdoc />
    public partial class Balance2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPremium",
                table: "Ads",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsVip",
                table: "Ads",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "PremiumUntil",
                table: "Ads",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "VipUntil",
                table: "Ads",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPremium",
                table: "Ads");

            migrationBuilder.DropColumn(
                name: "IsVip",
                table: "Ads");

            migrationBuilder.DropColumn(
                name: "PremiumUntil",
                table: "Ads");

            migrationBuilder.DropColumn(
                name: "VipUntil",
                table: "Ads");
        }
    }
}
