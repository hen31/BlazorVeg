using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Veg.Data.EntityFramework.SQL.Migrations
{
    public partial class MutationLoggingProduct : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BarcodeAddedById",
                table: "Products",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ImageAddedById",
                table: "Products",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_BarcodeAddedById",
                table: "Products",
                column: "BarcodeAddedById");

            migrationBuilder.CreateIndex(
                name: "IX_Products_ImageAddedById",
                table: "Products",
                column: "ImageAddedById");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Members_BarcodeAddedById",
                table: "Products",
                column: "BarcodeAddedById",
                principalTable: "Members",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Members_ImageAddedById",
                table: "Products",
                column: "ImageAddedById",
                principalTable: "Members",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Members_BarcodeAddedById",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Members_ImageAddedById",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_BarcodeAddedById",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_ImageAddedById",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "BarcodeAddedById",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ImageAddedById",
                table: "Products");
        }
    }
}
