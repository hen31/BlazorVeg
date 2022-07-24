using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veg.Entities;
using Veg.Storage;

namespace Veg.Repositories
{//Report
    public class ReportRepository : EFRepository<Report>
    {
        public ReportRepository(VegDatabaseContext databaseContext)
        {
            this.DbContext = databaseContext;
        }
        public override DbSet<Report> DbSet => DbContext.Reports;

        public override async Task<Report> GetItemByIdAsync(Guid id)
        {
            return await GetItemById(id, new string[] {
                "AddedByMember",
                "Product",
                "Product.Brand", "Product.StoresAvailable", "Product.StoresAvailable.Store",
                "Product.AddedByMember", "Product.Category", "Product.Category.ParentCategory",
                "Product.Category.ParentCategory.ParentCategory", "Product.Brand", "Product.Tags.Tag",
                "ProductReview",
                "ProductReview.Product",
                "ProductReview.Member",
                "ProductReview.ReviewImages"});
        }

        public override IQueryable<Report> ApplyFiltering(IQueryable<Report> query, Dictionary<string, string> filterValues)
        {
            bool onlyNotHandled = bool.Parse(filterValues["Handled"]);
            if (onlyNotHandled)
            {
                return query.Where(b => !b.Handled);
            }
            else
            {
                return query;
            }
        }

        public override void ConfigureModel(EntityTypeBuilder<Report> modelBuilder)
        {

        }
    }
}
