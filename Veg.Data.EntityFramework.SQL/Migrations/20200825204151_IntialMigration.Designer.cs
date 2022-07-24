﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Veg.Data.EntityFramework.SQL;

namespace Veg.Data.EntityFramework.SQL.Migrations
{
    [DbContext(typeof(SQLContext))]
    [Migration("20200825204151_IntialMigration")]
    partial class IntialMigration
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.7")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Veg.Entities.AvailableAt", b =>
                {
                    b.Property<Guid>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("AddedByMemberId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("DateAdded")
                        .HasColumnType("datetime2");

                    b.Property<Guid?>("ProductID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("StoreId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("ID");

                    b.HasIndex("AddedByMemberId");

                    b.HasIndex("ProductID");

                    b.HasIndex("StoreId");

                    b.ToTable("AvailableAt");
                });

            modelBuilder.Entity("Veg.Entities.Brand", b =>
                {
                    b.Property<Guid>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("AddedByMemberId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("BrandImage")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("DateAdded")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("ID");

                    b.HasIndex("AddedByMemberId");

                    b.ToTable("Brands");
                });

            modelBuilder.Entity("Veg.Entities.Member", b =>
                {
                    b.Property<Guid>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("EmailAdress")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("HasCustomProfileImage")
                        .HasColumnType("bit");

                    b.Property<bool>("IsAdmin")
                        .HasColumnType("bit");

                    b.Property<bool>("IsModerator")
                        .HasColumnType("bit");

                    b.Property<string>("UserName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("UserSince")
                        .HasColumnType("datetime2");

                    b.HasKey("ID");

                    b.ToTable("Members");
                });

            modelBuilder.Entity("Veg.Entities.Product", b =>
                {
                    b.Property<Guid>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("AddedByMemberId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("BrandId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("DateAdded")
                        .HasColumnType("datetime2");

                    b.Property<bool>("IsVegan")
                        .HasColumnType("bit");

                    b.Property<bool>("IsVegetarian")
                        .HasColumnType("bit");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ProductImage")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("ID");

                    b.HasIndex("AddedByMemberId");

                    b.HasIndex("BrandId");

                    b.ToTable("Products");
                });

            modelBuilder.Entity("Veg.Entities.ProductReview", b =>
                {
                    b.Property<Guid>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Content")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("MemberId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("ProductId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<byte>("Rating")
                        .HasColumnType("tinyint");

                    b.HasKey("ID");

                    b.HasIndex("MemberId");

                    b.HasIndex("ProductId");

                    b.ToTable("ProductReviews");
                });

            modelBuilder.Entity("Veg.Entities.ReviewImage", b =>
                {
                    b.Property<Guid>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("ImageName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("ReviewID")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("ID");

                    b.HasIndex("ReviewID");

                    b.ToTable("ReviewImage");
                });

            modelBuilder.Entity("Veg.Entities.Store", b =>
                {
                    b.Property<Guid>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("AddedByMemberId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("DateAdded")
                        .HasColumnType("datetime2");

                    b.Property<bool>("DefaultInSelection")
                        .HasColumnType("bit");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("ID");

                    b.HasIndex("AddedByMemberId");

                    b.ToTable("Stores");
                });

            modelBuilder.Entity("Veg.Entities.AvailableAt", b =>
                {
                    b.HasOne("Veg.Entities.Member", "AddedByMember")
                        .WithMany()
                        .HasForeignKey("AddedByMemberId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("Veg.Entities.Product", null)
                        .WithMany("StoresAvailable")
                        .HasForeignKey("ProductID");

                    b.HasOne("Veg.Entities.Store", "Store")
                        .WithMany()
                        .HasForeignKey("StoreId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Veg.Entities.Brand", b =>
                {
                    b.HasOne("Veg.Entities.Member", "AddedByMember")
                        .WithMany()
                        .HasForeignKey("AddedByMemberId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();
                });

            modelBuilder.Entity("Veg.Entities.Product", b =>
                {
                    b.HasOne("Veg.Entities.Member", "AddedByMember")
                        .WithMany()
                        .HasForeignKey("AddedByMemberId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("Veg.Entities.Brand", "Brand")
                        .WithMany("Products")
                        .HasForeignKey("BrandId");
                });

            modelBuilder.Entity("Veg.Entities.ProductReview", b =>
                {
                    b.HasOne("Veg.Entities.Member", "Member")
                        .WithMany()
                        .HasForeignKey("MemberId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("Veg.Entities.Product", "Product")
                        .WithMany("Reviews")
                        .HasForeignKey("ProductId");
                });

            modelBuilder.Entity("Veg.Entities.ReviewImage", b =>
                {
                    b.HasOne("Veg.Entities.ProductReview", "Review")
                        .WithMany("ReviewImages")
                        .HasForeignKey("ReviewID");
                });

            modelBuilder.Entity("Veg.Entities.Store", b =>
                {
                    b.HasOne("Veg.Entities.Member", "AddedByMember")
                        .WithMany()
                        .HasForeignKey("AddedByMemberId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
