using LigaNOS.Data.Entities;

namespace LigaNOS.Data
{
    public class GameRepository : GenericRepository<Game>, IGameRepository
    {
        public GameRepository(DataContext context) : base(context)
        {  
        }
    }
}
