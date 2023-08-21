using LigaNOS.Data.Entities;
using System.Linq;
using System.Threading.Tasks;

namespace LigaNOS.Data
{
    public interface IGameRepository : IGenericRepository<Game>
    {
        public IQueryable GetAllWithUsers();
    }
}
