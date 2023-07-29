using LigaNOS.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace LigaNOS.Data
{
    public class TeamRepository : GenericRepository<Team>, ITeamRepository
    {
        private readonly DataContext _context;

        public TeamRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        public IQueryable GetAllWithUsers()
        {
            return _context.Teams.Include(p => p.User);
        }
    }
}
