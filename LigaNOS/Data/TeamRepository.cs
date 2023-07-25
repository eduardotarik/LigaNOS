using LigaNOS.Data.Entities;

namespace LigaNOS.Data
{
    public class TeamRepository : GenericRepository<Team>, ITeamRepository
    {
        public TeamRepository(DataContext context) : base(context)
        {
        }
    }
}
