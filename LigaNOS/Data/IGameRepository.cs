using LigaNOS.Data.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LigaNOS.Data
{
    public interface IGameRepository : IGenericRepository<Game>
    {
        public IQueryable GetAllWithUsers();

        Task<bool> SaveAllAsync();

        Task ClearGamesAsync();

        Task<List<Game>> GetAllAsync();
    }
}
