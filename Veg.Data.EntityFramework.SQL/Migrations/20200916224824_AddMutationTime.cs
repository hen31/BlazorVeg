using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Veg.Data.EntityFramework.SQL.Migrations
{
    public partial class AddMutationTime : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReviewImage_ProductReviews_ReviewID",
                table: "ReviewImage");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfMutation",
                table: "ProductReviews",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddForeignKey(
                name: "FK_ReviewImage_ProductReviews_ReviewID",
                table: "ReviewImage",
                column: "ReviewID",
                principalTable: "ProductReviews",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReviewImage_ProductReviews_ReviewID",
                table: "ReviewImage");

            migrationBuilder.DropColumn(
                name: "DateOfMutation",
                table: "ProductReviews");

            migrationBuilder.AddForeignKey(
                name: "FK_ReviewImage_ProductReviews_ReviewID",
                table: "ReviewImage",
                column: "ReviewID",
                principalTable: "ProductReviews",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
