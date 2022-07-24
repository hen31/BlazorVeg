using Veg.Core;
using Veg.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Reflection;

namespace Veg.Storage
{
    public abstract class EFRepository : IRepository
    {
        public VegDatabaseContext DbContext
        {
            get; set;
        }

        public virtual Task<bool> CheckIfMemberMayChangeObject(BaseEntity baseEntity, Member member)
        {
            return Task.FromResult(false);
        }

        public virtual Task<bool> CheckIfMemberMayChangeObject(Guid guid, Member member)
        {
            return Task.FromResult(false);
        }

        public virtual Task<bool> CheckIfMemberMayChangeObject(Guid guid, BaseEntity baseEntity, Member member)
        {
            return Task.FromResult(false);
        }
    }
    public abstract class EFRepository<T> : EFRepository, IEFRepository<T> where T : BaseEntity, new()
    {
        public abstract void ConfigureModel(EntityTypeBuilder<T> modelBuilder);


        public static void ChangeDbContext(VegDatabaseContext databaseContext)
        {

        }

        public abstract DbSet<T> DbSet { get; }
        public virtual async Task<T> AddAsync(T entity)
        {
            BuildChangeGraph(entity);
            //await DbSet.AddAsync(entity);
            await DbContext.SaveChangesAsync().ConfigureAwait(false);
            UntrackItem(entity);
            return entity;
        }

        public virtual async Task<bool> DeleteAsync(T entity)
        {
            BeforeItemDeleted(entity);
            //DbContext.Attach(entity);
            DbContext.Entry(entity).State = EntityState.Deleted;
            DbSet.Remove(entity);
            await DbContext.SaveChangesAsync().ConfigureAwait(false);
            return true;
        }
        public virtual void BeforeItemDeleted(T entity)
        {

        }
        public abstract IQueryable<T> ApplyFiltering(IQueryable<T> query, Dictionary<string, string> filterValues);

        public virtual async Task<ICollection<T>> GetCollectionAsync(int from, int count, string sortField, bool desc, Dictionary<string, string> filterValues, string[] includes)
        {
            var collection = ApplyFiltering(DbSet, filterValues);
            if (includes != null)
            {
                foreach (string include in includes)
                {
                    collection = collection.Include(include);
                }
            }

            if (sortField != null)
            {
                collection = collection.OrderByDynamic(sortField, !desc);
            }
            collection = collection.Skip(from).Take(count);
            return await collection.AsNoTracking().ToListAsync().ConfigureAwait(false);
        }
        public static IQueryable<T> AddIncludes(IQueryable<T> query, string[] includes)
        {
            if (includes != null)
            {
                foreach (string include in includes)
                {
                    query = query.Include(include);
                }
            }
            return query;
        }
        public virtual async Task<T> GetItemByIdAsync(Guid id)
        {
            return await GetItemById(id, null);
        }

        public virtual async Task<T> GetItemById(Guid id, string[] includes)
        {
            if (includes == null || includes.Length == 0)
            {
                return await DbSet.FirstOrDefaultAsync(b => b.ID == id);
            }
            else
            {
                var querable = DbSet.Include(includes[0]);
                foreach (string include in includes.Skip(1))
                {
                    querable = querable.Include(include);
                }
                return await querable.FirstOrDefaultAsync(b => b.ID == id).ConfigureAwait(false);
            }

        }

        public virtual void CancelEdit(BaseEntity entity)
        {
            UntrackItem(entity);
        }

        public virtual void UntrackItem(BaseEntity entity)
        {
            DbContext.Entry(entity).State = EntityState.Detached;
            var entries = DbContext.ChangeTracker.Entries().ToList();
            foreach (EntityEntry dbEntityEntry in entries)
            {
                dbEntityEntry.State = EntityState.Detached;
            }
        }

        public virtual async Task<T> UpdateAsync(Guid id, T entity)
        {
            UntrackItem(entity);
            BuildChangeGraph(entity);
            //DbSet.Update(entity);
            foreach (var collection in DbContext.Entry(entity).Collections.ToList())
            {
                if (collection.Metadata.Name != "Reviews")
                {
                    var loadedEntity = DbSet.Where(b => b.ID == entity.ID).Include(collection.Metadata.Name).AsNoTracking().ToList().FirstOrDefault();
                    var dbCollection = (loadedEntity.GetType().GetProperty(collection.Metadata.Name).GetValue(loadedEntity) as IEnumerable<BaseEntity>).ToList();
                    var currenentValues = new List<BaseEntity>(collection.CurrentValue.Cast<BaseEntity>());
                    foreach (var itemInDb in dbCollection)
                    {
                        if (currenentValues.Where(b => b.ID == itemInDb.ID).Count() == 0)
                        {
                            DbContext.Entry(itemInDb).State = EntityState.Deleted;
                        }
                    }
                }
            }
            await DbContext.SaveChangesAsync();
            UntrackItem(entity);
            return await GetItemByIdAsync(entity.ID).ConfigureAwait(false);
        }

        protected void BuildChangeGraph(BaseEntity entity)
        {
            DbContext.ChangeTracker.TrackGraph(entity, node =>
            {
                var entry = node.Entry;
                var childEntity = entry.Entity;

                if (entry.IsKeySet)
                {
                    entry.State = EntityState.Modified;
                }
                else
                {
                    entry.State = EntityState.Added;
                }

            });
        }

        public async Task<T> AddSlowAsync(T entity)
        {
            await Task.Delay(2500);
            return await AddAsync(entity).ConfigureAwait(false);
        }

        public async Task<int> GetCountAsync(Dictionary<string, string> filterValues)
        {
            return await ApplyFiltering(DbSet, filterValues).CountAsync().ConfigureAwait(false);
        }

        public static ICollection<T> ToLowDetailCollection(ICollection<T> detailedCollection)
        {
            return detailedCollection.Select(b => ToLowDetailObject(b)).ToList();
        }
        public static ICollection<K> ToLowDetailCollection<K>(ICollection<K> detailedCollection) where K : BaseEntity
        {
            return detailedCollection.Select(b => ToLowDetailObjectGeneric<K>(b)).ToList();
        }

        /*   public static void HideEmailOfMember(BaseEntity entity)
           {
               var valueType = entity.GetType();
               var properties = valueType.GetProperties();
               foreach (var property in properties)
               {
                   if (property.Name == "EmailAdress")
                   {
                       property.SetValue(property.Name, "");
                   }
                   else
                   {
                       if (property.PropertyType.IsSubclassOf(typeof(BaseEntity)))
                       {
                           var valueOfSubEntity = property.GetValue(entity) as BaseEntity;
                           if (valueOfSubEntity != null)
                           {
                               HideEmailOfMember(valueOfSubEntity);
                           }
                       }
                   }
               }
           }*/

        public static T ToLowDetailObject(T detailedValue)
        {
            T copyWithLowDetail = new T();
            CopyValuesFromObject(detailedValue, copyWithLowDetail);
            return copyWithLowDetail;
        }
        public static K ToLowDetailObjectGeneric<K>(BaseEntity entity) where K : BaseEntity
        {
            BaseEntity instance = (BaseEntity)Activator.CreateInstance(entity.GetType());
            CopyValuesFromObject(entity, instance);
            return (K)instance;
        }

        private static void CopyValuesFromObject(BaseEntity fromEntity, BaseEntity copyToEntity)
        {
            var valueType = fromEntity.GetType();
            var properties = valueType.GetProperties();


            foreach (var property in properties)
            {
                if (property.GetCustomAttribute(typeof(HighDetailPropertyAttribute)) == null && property.SetMethod != null)
                {

                    if (property.PropertyType.IsSubclassOf(typeof(BaseEntity)))
                    {
                        var valueOfSubEntity = property.GetValue(fromEntity) as BaseEntity;
                        if (valueOfSubEntity != null)
                        {
                            property.SetValue(copyToEntity, ToLowDetailObjectGeneric<BaseEntity>(valueOfSubEntity));
                        }
                    }
                    else
                    {
                        property.SetValue(copyToEntity, property.GetValue(fromEntity));
                    }
                }
            }
        }

    }

    public static class EFExtensions
    {
        public static IQueryable<T> OrderByDynamic<T>(this IQueryable<T> q, string SortField, bool Ascending)
        {
            var param = Expression.Parameter(typeof(T), "p");
            var prop = Expression.Property(param, SortField);
            var exp = Expression.Lambda(prop, param);
            string method = Ascending ? "OrderBy" : "OrderByDescending";
            Type[] types = new Type[] { q.ElementType, exp.Body.Type };
            var mce = Expression.Call(typeof(Queryable), method, types, q.Expression, exp);
            return q.Provider.CreateQuery<T>(mce);
        }


    }
}
