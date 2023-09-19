using LigaNOS.Data.Entities;
using LigaNOS.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LigaNOS.Data
{
    public class DataContext : IdentityDbContext<User>
    {
        public DbSet<Team> Teams { get; set; }

        public DbSet<Player> Players { get; set; }

        public DbSet<Game> Games { get; set; }

        public DbSet<User> users { get; set; }

        public DbSet<RegistrationRequest> RegistrationRequests { get; set; }

        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure the relationship between Game and User
            modelBuilder.Entity<Game>()
                .HasOne(g => g.User)
                .WithMany(u => u.Games) // Assuming a navigation property in User for associated games
                .HasForeignKey(g => g.UserId)
                .OnDelete(DeleteBehavior.Cascade); // This line enables cascading delete
        }


    }
}
