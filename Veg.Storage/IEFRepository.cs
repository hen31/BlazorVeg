using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Veg.Entities;

namespace Veg.Storage
{
    public interface IEFRepository<T> where T : BaseEntity, new()
    {
        DbSet<T> DbSet { get; }

        Task<T> AddAsync(T entity);
        Task<T> AddSlowAsync(T entity);
        IQueryable<T> ApplyFiltering(IQueryable<T> query, Dictionary<string, string> filterValues);
        void BeforeItemDeleted(T entity);
        void CancelEdit(BaseEntity entity);
        void ConfigureModel(EntityTypeBuilder<T> modelBuilder);
        Task<bool> DeleteAsync(T entity);
        Task<ICollection<T>> GetCollectionAsync(int from, int count, string sortField, bool desc, Dictionary<string, string> filterValues, string[] includes);
        Task<int> GetCountAsync(Dictionary<string, string> filterValues);
        Task<T> GetItemById(Guid id, string[] includes);
        Task<T> GetItemByIdAsync(Guid id);
        void UntrackItem(BaseEntity entity);
        Task<T> UpdateAsync(Guid id, T entity);
    }
}