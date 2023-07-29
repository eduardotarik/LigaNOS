using LigaNOS.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace LigaNOS.Data
{
    public class GameRepository : GenericRepository<Game>, IGameRepository
    {
        private readonly DataContext _context;

        public GameRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        public IQueryable GetAllWithUsers()
        {
            return _context.Games.Include(p => p.User);
        }
    }
}
