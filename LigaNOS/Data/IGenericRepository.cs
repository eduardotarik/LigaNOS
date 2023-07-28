using LigaNOS.Data.Entities;
using System.Linq;
using System.Threading.Tasks;

namespace LigaNOS.Data
{
    public interface IGenericRepository<T> where T : class
    {
        IQueryable<T> GetAll();

        Task<T> GetByIdAsync(int id);

        Task<Team> GetByNameAsync(string name);

        Task CreateAsync(T entity);

        Task UpdateAsync(T entity);

        Task DeleteAsync(T entity);

        Task<bool> ExistAsync(int id);
    }
}
