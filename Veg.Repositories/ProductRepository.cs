using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Veg.Entities;
using Veg.Storage;

namespace Veg.Repositories
{
    public class ProductRepository : EFRepository<Product>
    {
        public ProductRepository(VegDatabaseContext databaseContext)
        {
            this.DbContext = databaseContext;
        }
        public override DbSet<Product> DbSet => DbContext.Products;


        public override IQueryable<Product> ApplyFiltering(IQueryable<Product> query, Dictionary<string, string> filterValues)
        {
            return query;
        }
        string[] IncludesForObject = new string[] { "" +
            "Brand", "StoresAvailable", "StoresAvailable.Store",
            "AddedByMember", "Category", "Category.ParentCategory",
            "Category.ParentCategory.ParentCategory", "Brand", "Tags.Tag" };
        public override async Task<Product> GetItemByIdAsync(Guid id)
        {
            var product = await base.GetItemById(id, IncludesForObject);
            var query = DbContext.ProductReviews.Where(b => b.ProductId == id);
            if (await query.CountAsync() != 0)
            {
                product.Rating = await query.AverageAsync(b => b.Rating);
            }
            return product;
        }


        public async Task<List<Store>> GetStoresForSelection()
        {
            return await DbContext.Stores.Where(b => b.DefaultInSelection).AsNoTracking().ToListAsync().ConfigureAwait(false);
        }

        public async Task<Product> AddNewProductFromSiteAsync(Product entity, Member addedBy)
        {
            var brandFromDb = await DbContext.Brands.FirstOrDefaultAsync(brand => brand.Name == entity.Brand.Name);
            if (brandFromDb == null)
            {
                Brand brand = new Brand();
                brand.AddedByMemberId = addedBy.ID;
                brand.DateAdded = DateTime.UtcNow;
                brand.Name = entity.Brand.Name;
                DbContext.Brands.Add(brand);
                await DbContext.SaveChangesAsync();
                brandFromDb = brand;
            }
            entity.Brand = null;
            entity.BrandId = brandFromDb.ID;
            if (entity.Category != null && entity.Category.ID != Guid.Empty)
            {
                entity.CategoryId = entity.Category.ID;
                entity.Category = null;
            }
            entity.DateAdded = DateTime.UtcNow;
            entity.AddedByMember = null;
            entity.AddedByMemberId = addedBy.ID;
            foreach (var storeAvaibleAt in entity.StoresAvailable)
            {
                var storeFromDb = await DbContext.Stores.FirstOrDefaultAsync(store => store.Name == storeAvaibleAt.Store.Name);
                if (storeFromDb == null)
                {
                    Store store = new Store();
                    store.AddedByMemberId = addedBy.ID;
                    store.Name = storeAvaibleAt.Store.Name;
                    store.DateAdded = DateTime.UtcNow;
                    DbContext.Stores.Add(store);
                    await DbContext.SaveChangesAsync();
                    storeFromDb = store;
                }

                storeAvaibleAt.Store = null;
                storeAvaibleAt.StoreId = storeFromDb.ID;
                storeAvaibleAt.AddedByMember = null;
                storeAvaibleAt.AddedByMemberId = addedBy.ID;
                storeAvaibleAt.DateAdded = DateTime.UtcNow;

            }
            for (int i = 0; i < entity.Tags.Count; i++)
            {
                var tagLink = entity.Tags.ElementAt(i);
                Tag tagFromDb;
                if (tagLink.Tag == null)
                {
                    tagFromDb = await DbContext.Tags.Where(b => b.ID == tagLink.TagId).FirstOrDefaultAsync();
                }
                else
                {
                    tagFromDb = await DbContext.Tags.Where(b => b.Name == tagLink.Tag.Name).FirstOrDefaultAsync();
                }
                if (tagFromDb != null)
                {
                    tagLink.TagId = tagFromDb.ID;
                    tagLink.Tag = null;
                }
            }
            return await AddAsync(entity);
        }

        public async Task PrepareUpdate(Product entity, Member addedBy)
        {
            entity.AddedByMember = null;
            entity.ImageAddedBy = null;
            entity.BarcodeAddedBy = null;
            entity.Brand.AddedByMember = null;

            foreach (var storeAvaibleAt in entity.StoresAvailable)
            {
                var storeFromDb = await DbContext.Stores.AsNoTracking().FirstOrDefaultAsync(store => store.Name == storeAvaibleAt.Store.Name);
                if (storeFromDb == null)
                {
                    Store store = new Store();
                    store.AddedByMemberId = addedBy.ID;
                    store.AddedByMember = null;
                    store.Name = storeAvaibleAt.Store.Name;
                    store.DateAdded = DateTime.UtcNow;
                    DbContext.Stores.Add(store);
                    await DbContext.SaveChangesAsync();
                    storeFromDb = store;
                }

                storeAvaibleAt.Store = null;
                storeAvaibleAt.StoreId = storeFromDb.ID;
                storeAvaibleAt.AddedByMember = null;
                storeAvaibleAt.AddedByMemberId = addedBy.ID;
                storeAvaibleAt.DateAdded = DateTime.UtcNow;
            }
            for (int i = 0; i < entity.Tags.Count; i++)
            {
                var tagLink = entity.Tags.ElementAt(i);
                Tag tagFromDb;
                if (tagLink.Tag == null)
                {
                    tagFromDb = await DbContext.Tags.Where(b => b.ID == tagLink.TagId).FirstOrDefaultAsync();
                }
                else
                {
                    tagFromDb = await DbContext.Tags.Where(b => b.Name == tagLink.Tag.Name).FirstOrDefaultAsync();
                }
                if (tagFromDb != null)
                {
                    tagLink.TagId = tagFromDb.ID;
                    tagLink.Tag = null;
                }
            }
        }
        public async Task<ICollection<Product>> GetProductsForMemberAsync(Member member, int page)
        {
            IQueryable<Product> products = DbContext.Products.Where(b => b.AddedByMemberId == member.ID).OrderByDescending(b => b.DateAdded);
            products = products.Include(IncludesForObject[0]);
            foreach (string include in IncludesForObject.Skip(1))
            {
                products = products.Include(include);
            }
            if (page != 0)
            {
                products = products.Skip(page * 10);
            }
            products = products.Take(10);
            return await products.ToListAsync();
        }

        public async Task<int> GetReviewsForMemberCountAsync(Member member)
        {
            IQueryable<ProductReview> reviewsOfProduct = DbContext.ProductReviews.Where(b => b.MemberId == member.ID)
               .Include(b => b.ReviewImages).Include(b => b.Member).Include(b => b.Product);
            return await reviewsOfProduct.CountAsync();
        }

        public async Task<int> GetProductsForMemberCountAsync(Member member)
        {
            IQueryable<Product> products = DbContext.Products.Where(b => b.AddedByMemberId == member.ID).OrderByDescending(b => b.DateAdded);
            products = products.Include(IncludesForObject[0]);
            foreach (string include in IncludesForObject.Skip(1))
            {
                products = products.Include(include);
            }
            return await products.CountAsync();
        }

        public async Task<ICollection<ProductReview>> GetReviewsForMemberAsync(Member member, int page)
        {
            IQueryable<ProductReview> reviewsOfProduct = DbContext.ProductReviews.Where(b => b.MemberId == member.ID)
                .Include(b => b.ReviewImages).Include(b => b.Member).Include(b => b.Product).ThenInclude(b => b.Brand);
            reviewsOfProduct = reviewsOfProduct.OrderByDescending(b => b.DateAdded);
            if (page != 0)
            {
                reviewsOfProduct = reviewsOfProduct.Skip(page * 10);
            }
            reviewsOfProduct = reviewsOfProduct.Take(10);
            return await reviewsOfProduct.ToListAsync();
        }

        public async Task<ReviewSearchResultViewModel> GetReviewsForProductAsync(Product product, int page, int selectedSorting)
        {
            ReviewSearchResultViewModel searchResultViewModel = new ReviewSearchResultViewModel();
            IQueryable<ProductReview> reviewsOfProduct = DbContext.ProductReviews.Where(b => b.ProductId == product.ID).Include(b => b.ReviewImages).Include(b => b.Member);

            if (selectedSorting == 1)
            {
                reviewsOfProduct = reviewsOfProduct.OrderByDescending(b => b.Rating).ThenByDescending(b => b.DateAdded);
            }
            else if (selectedSorting == 2)
            {
                reviewsOfProduct = reviewsOfProduct.OrderBy(b => b.Rating).ThenByDescending(b => b.DateAdded);
            }
            else
            {
                reviewsOfProduct = reviewsOfProduct.OrderByDescending(b => b.DateAdded);
            }

            searchResultViewModel.TotalCount = await reviewsOfProduct.CountAsync();
            if (page != 0)
            {
                reviewsOfProduct = reviewsOfProduct.Skip(page * 5);
            }
            reviewsOfProduct = reviewsOfProduct.Take(5);
            searchResultViewModel.Reviews = await reviewsOfProduct.ToListAsync();
            searchResultViewModel.Page = page;
            return searchResultViewModel;
        }

        public async Task<SearchResultsViewmodel> SearchForProducts(string searchTerm, Guid selectedCategory, bool onlyVegan, string sortBy, int page, int pageSize, LimitResultOptions limitResultOptions)
        {
            SearchResultsViewmodel viewmodel = new SearchResultsViewmodel();
            viewmodel.PageSize = pageSize;
            List<Guid> categories = new List<Guid>();
            if (selectedCategory != Guid.Empty)
            {
                categories = await DbContext.ProductCategory.Where(b => b.ID == selectedCategory
                || b.ParentCategoryId == selectedCategory
                || (b.ParentCategory != null && b.ParentCategory.ParentCategoryId == selectedCategory))
                    .Select(b => b.ID).ToListAsync();
            }
            var queryForAllProducts = DbContext.Products.Include(b => b.Brand).Include(b => b.Category).ThenInclude(b => b.ParentCategory)
                .ThenInclude(b => b.ParentCategory)
                .Include(b => b.StoresAvailable)
                .ThenInclude(b => b.Store)
                .Where(b => (!onlyVegan || b.IsVegan)
                           && (selectedCategory == Guid.Empty
                           || categories.Contains(b.CategoryId))
                         ).Select(b => b);
            if (limitResultOptions.OnlyWithoutImage)
            {
                queryForAllProducts = queryForAllProducts.Where(b => string.IsNullOrWhiteSpace(b.ProductImage));
            }
            if (limitResultOptions.OnlyNotVerified)
            {
                queryForAllProducts = queryForAllProducts.Where(b => !b.ImageVerified && !string.IsNullOrWhiteSpace(b.ProductImage));
            }
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                string[] parts = searchTerm.Split(' ');
                foreach (var part in parts)
                {
                    queryForAllProducts = queryForAllProducts.Where(b => (EF.Functions.Like(b.Name, $"%{ part }%")
                          || EF.Functions.Like(b.Brand.Name, $"%{ part }%") || b.Tags.Select(b => b.Tag.Name).Contains(part)));
                }

            }
            var tagString = string.Join(",", limitResultOptions.Tags.Select(b => b.ToString("D")).ToArray());
            var queryWithLimitingOptions = queryForAllProducts.Where(b => (limitResultOptions.Brands.Count == 0 || limitResultOptions.Brands.Contains(b.BrandId))
                                                                    && (limitResultOptions.ProductCategories.Count == 0 || limitResultOptions.ProductCategories.Contains(b.CategoryId))
                                                                    && (limitResultOptions.Tags.Count == 0 || b.Tags.Any(b => limitResultOptions.Tags.Contains(b.TagId))));
            viewmodel.TotalItems = await queryWithLimitingOptions.CountAsync();

            if (limitResultOptions.Sorting == "HighLow")
            {
                queryWithLimitingOptions = queryWithLimitingOptions.Include(b => b.Reviews);
                queryWithLimitingOptions = queryWithLimitingOptions.OrderByDescending(b => b.Reviews.Average(c => c.Rating)).ThenBy(b => b.Brand.Name).ThenBy(b => b.Name);
            }
            else if (limitResultOptions.Sorting == "LowHigh")
            {
                queryWithLimitingOptions = queryWithLimitingOptions.Include(b => b.Reviews);
                queryWithLimitingOptions = queryWithLimitingOptions.OrderBy(b => b.Reviews.Average(c => c.Rating)).ThenBy(b => b.Brand.Name).ThenBy(b => b.Name);
            }
            else
            {
                queryWithLimitingOptions = queryWithLimitingOptions.OrderBy(b => b.Brand.Name).ThenBy(b => b.Name);
            }

            viewmodel.Products = await queryWithLimitingOptions.Skip(page * pageSize).Take(pageSize).ToListAsync();
            foreach (var product in viewmodel.Products)
            {
                product.Rating = await GetRatingForProduct(product);
            }
            viewmodel.Brands = (await queryForAllProducts.Select(b => b.Brand).Distinct().ToListAsync()).OrderBy(b => b.Name).ToList();
            viewmodel.Categories = (await queryForAllProducts.Select(b => b.Category).Distinct().ToListAsync()).OrderBy(b => b.FullCategoryPath).ToList();
            viewmodel.Tags = (await queryForAllProducts.SelectMany(b => b.Tags.Select(b => b.Tag)).Distinct().ToListAsync()).OrderBy(b => b.Name).ToList();
            viewmodel.CurrentPage = page;
            viewmodel.SortedBy = sortBy;
            return viewmodel;
        }



        public override async Task<Product> UpdateAsync(Guid id, Product entity)
        {


            return await base.UpdateAsync(id, entity);
        }

        public async Task<Product> GetProductFromBarcodeAsync(string barcode)
        {
            return await AddIncludes(DbSet.Where(b => b.Barcode == barcode), IncludesForObject).FirstOrDefaultAsync();
        }
        public override async Task<bool> DeleteAsync(Product entity)
        {
            foreach (var avaibleAt in entity.StoresAvailable)
            {
                DbContext.Entry(avaibleAt).State = EntityState.Deleted;
            }
            foreach (var tag in entity.Tags)
            {
                DbContext.Entry(tag).State = EntityState.Deleted;
            }
            foreach (var review in await DbContext.ProductReviews.Where(b => b.ProductId == entity.ID).Include(b => b.ReviewImages).ToListAsync())
            {
                foreach (var reviewImage in review.ReviewImages)
                {
                    DbContext.Entry(reviewImage).State = EntityState.Deleted;
                }
                DbContext.Entry(review).State = EntityState.Deleted;
            }
            return await base.DeleteAsync(entity);
        }

        public async Task<ProductReview> GetReviewByIdAsync(Guid reviewid)
        {
            return await DbContext.ProductReviews.Include(b => b.Member).Include(b => b.ReviewImages).Include(b => b.Product).FirstOrDefaultAsync(b => b.ID == reviewid);
        }

        public async Task DeleteReviewAsync(ProductReview review)
        {
            foreach (var image in review.ReviewImages)
            {
                DbContext.Entry(image).State = EntityState.Deleted;
            }
            DbContext.Entry(review).State = EntityState.Deleted;
            await DbContext.SaveChangesAsync();
        }

        public async Task<Product> AddBarcodeToProductAsync(Product product, string barcode, Guid mutatedBy)
        {
            var dbProduct = await DbSet.FirstOrDefaultAsync(b => b.ID == product.ID);
            dbProduct.Barcode = barcode;
            dbProduct.BarcodeAddedById = mutatedBy;
            DbContext.Entry(dbProduct).State = EntityState.Modified;
            await DbContext.SaveChangesAsync();
            return await GetItemByIdAsync(product.ID);
        }

        public async Task<ProductReview> UpdateReviewAsync(Guid reviewId, ProductReview entity)
        {
            entity.Member = null;
            entity.Product = null;
            if (entity.ReviewImages != null)
            {
                foreach (var image in entity.ReviewImages)
                {
                    image.Review = null;
                }
            }
            UntrackItem(entity);
            BuildChangeGraph(entity);
            //DbSet.Update(entity);
            foreach (var collection in DbContext.Entry(entity).Collections.ToList())
            {
                var loadedEntity = DbContext.ProductReviews.Where(b => b.ID == entity.ID).Include(collection.Metadata.Name).AsNoTracking().ToList().FirstOrDefault();
                var dbCollection = (loadedEntity.GetType().GetProperty(collection.Metadata.Name).GetValue(loadedEntity) as IEnumerable<BaseEntity>).ToList();
                var currenentValues = new List<BaseEntity>(collection.CurrentValue.Cast<BaseEntity>());
                foreach (var itemInDb in dbCollection)
                {
                    if (currenentValues.Where(b => b.ID == itemInDb.ID).Count() == 0)
                    {
                        /* var trackedEntry = DbContext.ChangeTracker.Entries().Where(b => (b.Entity as BaseEntity).ID == itemInDb.ID).FirstOrDefault();
                        if()*/
                        DbContext.Entry(itemInDb).State = EntityState.Deleted;
                    }
                }
            }
            await DbContext.SaveChangesAsync();
            UntrackItem(entity);
            return await GetReviewByIdAsync(entity.ID).ConfigureAwait(false);
        }

        public async Task<Product> AddPhotoToProductAsync(Product product, string photoName, Guid addedBy)
        {
             var dbProduct = await DbSet.AsNoTracking().FirstOrDefaultAsync(b => b.ID == product.ID);
            dbProduct.ProductImage = photoName;
            dbProduct.AddedByMember = null;
            dbProduct.ImageAddedById = addedBy;
            DbContext.Entry(dbProduct).State = EntityState.Modified;
            await DbContext.SaveChangesAsync();
            return await GetItemByIdAsync(dbProduct.ID);
        }

        public async Task<Product> DeletePhotoToProductAsync(Guid productid)
        {
            var dbProduct = await DbContext.Products.AsNoTracking().FirstOrDefaultAsync(b => b.ID == productid);
            dbProduct.ProductImage = null;
            dbProduct.AddedByMember = null;
            DbContext.Entry(dbProduct).State = EntityState.Modified;
            await DbContext.SaveChangesAsync();
            return await GetItemByIdAsync(dbProduct.ID);
        }

        public async Task<Product> SetImageVerified(Guid productid)
        {
            var dbProduct = await DbContext.Products.AsNoTracking().FirstOrDefaultAsync(b => b.ID == productid);
            dbProduct.AddedByMember = null;
            dbProduct.ImageVerified = true;
            DbContext.Entry(dbProduct).State = EntityState.Modified;
            await DbContext.SaveChangesAsync();
            return await GetItemByIdAsync(dbProduct.ID);
        }

        private async Task<double> GetRatingForProduct(Product product)
        {
            return await DbContext.ProductReviews.Where(b => b.ProductId == product.ID).AverageAsync(b => (int?)b.Rating) ?? 0;
        }

        public async Task<ProductReview> ChangeReviewOfProductForUserAsync(Product product, ProductReview review, Member addedBy)
        {
            var oldReviewFromDb = await GetReviewForProductOfMemberAsync(product, addedBy);
            DbContext.Entry(oldReviewFromDb).State = EntityState.Modified;
            oldReviewFromDb.Content = review.Content;
            oldReviewFromDb.Rating = review.Rating;
            oldReviewFromDb.DateOfMutation = DateTime.UtcNow;
            oldReviewFromDb.Member = null;
            int indexOfChangedImages = 0;
            foreach (var reviewImage in review.ReviewImages)
            {
                if (indexOfChangedImages < oldReviewFromDb.ReviewImages.Count)
                {
                    oldReviewFromDb.ReviewImages.ElementAt(indexOfChangedImages).ImageName = reviewImage.ImageName;
                    DbContext.Entry(oldReviewFromDb.ReviewImages.ElementAt(indexOfChangedImages)).State = EntityState.Modified;
                }
                else
                {
                    oldReviewFromDb.ReviewImages.Add(reviewImage);
                    DbContext.Entry(reviewImage).State = EntityState.Added;

                }
                indexOfChangedImages++;
            }
            if (indexOfChangedImages < oldReviewFromDb.ReviewImages.Count)
            {
                for (int i = indexOfChangedImages; i < oldReviewFromDb.ReviewImages.Count; i++)
                {
                    var oldImage = oldReviewFromDb.ReviewImages.ElementAt(i);
                    oldReviewFromDb.ReviewImages.Remove(oldImage);
                    DbContext.Entry(oldImage).State = EntityState.Deleted;
                }
            }
            await DbContext.SaveChangesAsync();
            return oldReviewFromDb;
        }

        public async Task<ProductReview> GetReviewForProductOfMemberAsync(Product product, Member addedBy)
        {
            return await DbContext.ProductReviews.Include(b => b.ReviewImages).FirstOrDefaultAsync(b => b.ProductId == product.ID && b.MemberId == addedBy.ID);
        }

        public async Task<ProductReview> AddReviewForProductAsync(Product product, ProductReview review)
        {
            review.ProductId = product.ID;
            review.Product = null;
            review.DateOfMutation = DateTime.UtcNow;
            review.Member = null;
            review.DateAdded = DateTime.UtcNow;
            DbContext.ProductReviews.Add(review);
            await DbContext.SaveChangesAsync();
            return review;
        }

        public async Task<ICollection<ProductCategory>> GetCategoriesForAutoSelect()
        {
            return ToLowDetailCollection<ProductCategory>((await DbContext.ProductCategory.Include(b => b.SubCategories).Include(b => b.ParentCategory).ThenInclude(b => b.ParentCategory).AsNoTracking().ToListAsync().ConfigureAwait(false)).OrderBy(b => b.FullCategoryPath).ToList());
        }

        public async Task<ICollection<Product>> GetLatestAddedProducts(int amount)
        {
            return ToLowDetailCollection((await DbSet.Where(b => !string.IsNullOrWhiteSpace(b.ProductImage)).OrderBy(b => b.DateAdded).AsNoTracking().Take(amount).Include(b => b.Brand).Include(b => b.AddedByMember).ToListAsync().ConfigureAwait(false)));
        }
        public async Task<List<Brand>> GetBrandsForSelection()
        {
            return await DbContext.Brands.AsNoTracking().ToListAsync().ConfigureAwait(false);
        }

        public async Task<List<Store>> GetStoresForAutoComplete()
        {
            return await DbContext.Stores.Where(b => !b.DefaultInSelection).AsNoTracking().ToListAsync().ConfigureAwait(false);
        }

        public override void ConfigureModel(EntityTypeBuilder<Product> modelBuilder)
        {
        }
    }
}
