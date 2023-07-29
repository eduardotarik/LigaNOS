using LigaNOS.Data.Entities;
using System.Linq;

namespace LigaNOS.Data
{
    public interface IGameRepository : IGenericRepository<Game>
    {
        public IQueryable GetAllWithUsers();
    }
}
