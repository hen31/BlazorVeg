using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Veg.Core
{
    public interface IDatabaseRepository<T> : IRepository
    {
        Task<T> AddAsync(T entity);
        Task<bool> DeleteAsync(T entity);
        Task<ICollection<T>> GetCollectionAsync(int from, int count, string sortField, bool desc, Dictionary<string, string> filterValues);
        Task<int> GetCountAsync();
        Task<T> GetItemByIdAsync(Guid id);
        Task<T> UpdateAsync(Guid id, T entity);
    }
}
