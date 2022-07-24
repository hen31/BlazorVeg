using BalanceKeeper.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace COS.Storage
{
    public class EFCategoryRepository : EFRepository<Category>, ICategoryRepository
    {
        public override DbSet<Category> DbSet => DbContext.Categories;

        public async Task FindCategoriesForRelation(Transaction transaction, Relation relation)
        {
            foreach (Category category in await DbSet.Include(b => b.MatchWithTransactionDescription).ToListAsync())
            {
                foreach (var matchDescription in category.MatchWithTransactionDescription)
                {
                    if (!string.IsNullOrWhiteSpace(matchDescription.Pattern))
                    {
                        if (Regex.IsMatch(transaction.Statement.ToLowerInvariant(), WildcardToRegex(matchDescription.Pattern.ToLowerInvariant()))
                            || Regex.IsMatch(relation.Name.ToLowerInvariant(), WildcardToRegex(matchDescription.Pattern.ToLowerInvariant())))
                        {
                          

                            relation.CategoryLinks.Add(new CategoryRelationLink()
                            {
                                CategoryID = matchDescription.CategoryID,
                                Percentage = matchDescription.Percentage,
                                Relation = relation,
                                UserId = ServiceResolver.GetService<IUserProvider>().GetUserId()
                            });
                            break;
                        }
                    }
                }
            }
        }

        public static string WildcardToRegex(string pattern)
        {
            return "^" + Regex.Escape(pattern).
            Replace("\\*", ".*").
            Replace("\\?", ".") + "$";
        }

        public override async Task<ICollection<Category>> GetCollectionAsync()
        {
            return await DbSet.Include(b => b.MainCategory).Include(b => b.MatchWithTransactionDescription).AsNoTracking().ToListAsync();
        }

        public async Task<ICollection<Category>> GetCollectionAsync(string searchTerm, long mainCategoryId)
        {
            IQueryable<Category> query = DbSet.Include(b => b.MainCategory);
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(c => EF.Functions.Like(c.Name, "%" + searchTerm + "%"));
            }
            if (mainCategoryId > 0)
            {
                query = query.Where(c => c.MainCategoryID == mainCategoryId);
            }
            return await query.Include(b => b.MainCategory).Include(b => b.MatchWithTransactionDescription).AsNoTracking().ToListAsync();
        }

        public override Task<Category> GetItemByIdAsync(long id)
        {
            return base.GetItemById(id, new string[] { "MainCategory", "MatchWithTransactionDescription" });
        }
    }



}
