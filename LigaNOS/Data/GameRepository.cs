using LigaNOS.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public async Task ClearGamesAsync()
        {
            var allGames = _context.Games.ToList();
            _context.Games.RemoveRange(allGames);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> SaveAllAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<List<Game>> GetAllAsync()
        {
            return await _context.Games.ToListAsync();
        }
    }
}
