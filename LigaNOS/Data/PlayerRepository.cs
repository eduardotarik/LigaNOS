using LigaNOS.Data.Entities;

namespace LigaNOS.Data
{
    public class PlayerRepository : GenericRepository<Player>, IPlayerRepository
    {
        public PlayerRepository(DataContext context) : base(context)
        { 
        }
    }
}
