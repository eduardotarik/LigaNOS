using LigaNOS.Data.Entities;
using System.Linq;
using System.Threading.Tasks;

namespace LigaNOS.Data
{
    public interface ITeamRepository : IGenericRepository<Team>
    {
        public IQueryable GetAllWithUsers();
    }
}
