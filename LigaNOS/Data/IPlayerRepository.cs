using LigaNOS.Data.Entities;
using System.Linq;

namespace LigaNOS.Data
{
    public interface IPlayerRepository : IGenericRepository<Player>
    {
        public IQueryable GetAllWithUsers();
    }
}
