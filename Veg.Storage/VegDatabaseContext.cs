using Veg.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Veg.Storage
{
    public abstract class VegDatabaseContext : DbContext
    {
        public void InitializeDbContext()
        {
            //this.Database.EnsureCreated();
            this.Database.Migrate();

        }
        public DbSet<Member> Members { get; set; }

        public DbSet<Product> Products { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<ProductReview> ProductReviews { get; set; }
        public DbSet<Store> Stores { get; set; }
        public DbSet<ProductCategory> ProductCategory { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Tag> Tags { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //DebugLoggerFactoryExtensions.AddDebug(MyLoggerFactory);
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.EnableSensitiveDataLogging(true);
            optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            optionsBuilder.UseLoggerFactory(LoggerFactory.Create(builder => builder.AddDebug().AddConsole()));
            //ChangeTracker.AutoDetectChangesEnabled = false;
            //ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>()
                        .HasMany(c => c.Reviews)
                        .WithOne(e => e.Product)
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired(false);

            modelBuilder.Entity<Product>()
                        .HasMany(c => c.StoresAvailable)
                        .WithOne()
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired(false);

            modelBuilder.Entity<Brand>()
                        .HasMany(c => c.Products)
                        .WithOne(e => e.Brand)
                        .IsRequired(false);
            modelBuilder.Entity<ProductReview>()
                        .HasMany(c => c.ReviewImages)
                        .WithOne(e => e.Review)
                        .IsRequired(false)
                        .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<ProductReview>()
                        .HasOne(c => c.Member)
                        .WithMany()
                        .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Store>()
                       .HasOne(c => c.AddedByMember)
                       .WithMany()
                       .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<AvailableAt>()
                       .HasOne(c => c.AddedByMember)
                       .WithMany()
                       .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Product>()
                       .HasOne(c => c.AddedByMember)
                       .WithMany()
                       .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Brand>()
                       .HasOne(c => c.AddedByMember)
                       .WithMany()
                       .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Product>()
                        .HasOne(c => c.Category)
                        .WithMany()
                        .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProductCategory>()
                        .HasOne(c => c.ParentCategory)
                        .WithMany(c => c.SubCategories)
                        .OnDelete(DeleteBehavior.Restrict);

        }

        public override int SaveChanges()
        {
            return base.SaveChanges();
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken))
        {
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
