using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Veg.Data.EntityFramework.SQL.Migrations
{
    public partial class IntialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Members",
                columns: table => new
                {
                    ID = table.Column<Guid>(nullable: false),
                    UserName = table.Column<string>(nullable: true),
                    UserSince = table.Column<DateTime>(nullable: false),
                    HasCustomProfileImage = table.Column<bool>(nullable: false),
                    IsAdmin = table.Column<bool>(nullable: false),
                    IsModerator = table.Column<bool>(nullable: false),
                    EmailAdress = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Members", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Brands",
                columns: table => new
                {
                    ID = table.Column<Guid>(nullable: false),
                    BrandImage = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    AddedByMemberId = table.Column<Guid>(nullable: false),
                    DateAdded = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Brands", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Brands_Members_AddedByMemberId",
                        column: x => x.AddedByMemberId,
                        principalTable: "Members",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Stores",
                columns: table => new
                {
                    ID = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    DefaultInSelection = table.Column<bool>(nullable: false),
                    AddedByMemberId = table.Column<Guid>(nullable: false),
                    DateAdded = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stores", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Stores_Members_AddedByMemberId",
                        column: x => x.AddedByMemberId,
                        principalTable: "Members",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    ID = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    BrandId = table.Column<Guid>(nullable: false),
                    IsVegan = table.Column<bool>(nullable: false),
                    IsVegetarian = table.Column<bool>(nullable: false),
                    ProductImage = table.Column<string>(nullable: true),
                    AddedByMemberId = table.Column<Guid>(nullable: false),
                    DateAdded = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Products_Members_AddedByMemberId",
                        column: x => x.AddedByMemberId,
                        principalTable: "Members",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Products_Brands_BrandId",
                        column: x => x.BrandId,
                        principalTable: "Brands",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AvailableAt",
                columns: table => new
                {
                    ID = table.Column<Guid>(nullable: false),
                    StoreId = table.Column<Guid>(nullable: false),
                    DateAdded = table.Column<DateTime>(nullable: false),
                    AddedByMemberId = table.Column<Guid>(nullable: false),
                    ProductID = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AvailableAt", x => x.ID);
                    table.ForeignKey(
                        name: "FK_AvailableAt_Members_AddedByMemberId",
                        column: x => x.AddedByMemberId,
                        principalTable: "Members",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AvailableAt_Products_ProductID",
                        column: x => x.ProductID,
                        principalTable: "Products",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AvailableAt_Stores_StoreId",
                        column: x => x.StoreId,
                        principalTable: "Stores",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductReviews",
                columns: table => new
                {
                    ID = table.Column<Guid>(nullable: false),
                    MemberId = table.Column<Guid>(nullable: false),
                    Content = table.Column<string>(nullable: true),
                    Rating = table.Column<byte>(nullable: false),
                    ProductId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductReviews", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ProductReviews_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductReviews_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReviewImage",
                columns: table => new
                {
                    ID = table.Column<Guid>(nullable: false),
                    ReviewID = table.Column<Guid>(nullable: false),
                    ImageName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewImage", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ReviewImage_ProductReviews_ReviewID",
                        column: x => x.ReviewID,
                        principalTable: "ProductReviews",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AvailableAt_AddedByMemberId",
                table: "AvailableAt",
                column: "AddedByMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_AvailableAt_ProductID",
                table: "AvailableAt",
                column: "ProductID");

            migrationBuilder.CreateIndex(
                name: "IX_AvailableAt_StoreId",
                table: "AvailableAt",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_Brands_AddedByMemberId",
                table: "Brands",
                column: "AddedByMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductReviews_MemberId",
                table: "ProductReviews",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductReviews_ProductId",
                table: "ProductReviews",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_AddedByMemberId",
                table: "Products",
                column: "AddedByMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_BrandId",
                table: "Products",
                column: "BrandId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewImage_ReviewID",
                table: "ReviewImage",
                column: "ReviewID");

            migrationBuilder.CreateIndex(
                name: "IX_Stores_AddedByMemberId",
                table: "Stores",
                column: "AddedByMemberId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AvailableAt");

            migrationBuilder.DropTable(
                name: "ReviewImage");

            migrationBuilder.DropTable(
                name: "Stores");

            migrationBuilder.DropTable(
                name: "ProductReviews");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Brands");

            migrationBuilder.DropTable(
                name: "Members");
        }
    }
}
