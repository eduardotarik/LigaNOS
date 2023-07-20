using LigaNOS.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace LigaNOS.Data
{
    public class DataContext : DbContext
    {
        public DbSet<Team> Teams { get; set; }

        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }
    }
}
