using LigaNOS.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace LigaNOS.Data
{
    public class PlayerRepository : GenericRepository<Player>, IPlayerRepository
    {
        private readonly DataContext _context;

        public PlayerRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        public IQueryable GetAllWithUsers()
        {
            return _context.Players.Include(p => p.User);
        }
    }
}
