using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Veg.Data.EntityFramework.SQL.Migrations
{
    public partial class WithLink : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tag_Products_ProductID",
                table: "Tag");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tag",
                table: "Tag");

            migrationBuilder.DropIndex(
                name: "IX_Tag_ProductID",
                table: "Tag");

            migrationBuilder.DropColumn(
                name: "ProductID",
                table: "Tag");

            migrationBuilder.RenameTable(
                name: "Tag",
                newName: "Tags");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tags",
                table: "Tags",
                column: "ID");

            migrationBuilder.CreateTable(
                name: "ProductTagLink",
                columns: table => new
                {
                    ID = table.Column<Guid>(nullable: false),
                    TagId = table.Column<Guid>(nullable: false),
                    ProductID = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductTagLink", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ProductTagLink_Products_ProductID",
                        column: x => x.ProductID,
                        principalTable: "Products",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductTagLink_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductTagLink_ProductID",
                table: "ProductTagLink",
                column: "ProductID");

            migrationBuilder.CreateIndex(
                name: "IX_ProductTagLink_TagId",
                table: "ProductTagLink",
                column: "TagId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductTagLink");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tags",
                table: "Tags");

            migrationBuilder.RenameTable(
                name: "Tags",
                newName: "Tag");

            migrationBuilder.AddColumn<Guid>(
                name: "ProductID",
                table: "Tag",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tag",
                table: "Tag",
                column: "ID");

            migrationBuilder.CreateIndex(
                name: "IX_Tag_ProductID",
                table: "Tag",
                column: "ProductID");

            migrationBuilder.AddForeignKey(
                name: "FK_Tag_Products_ProductID",
                table: "Tag",
                column: "ProductID",
                principalTable: "Products",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
