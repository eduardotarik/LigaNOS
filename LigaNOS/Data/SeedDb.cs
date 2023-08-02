using LigaNOS.Data.Entities;
using LigaNOS.Helpers;
using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LigaNOS.Data
{
    public class SeedDb
    {
        private readonly DataContext _context;
        private readonly IUserHelper _userHelper;
        private Random _random;

        public SeedDb(DataContext context, IUserHelper userHelper)
        {
            _context = context;
            _userHelper = userHelper;
            _random = new Random();
        }

        public async Task SeedAsync()
        {
            await _context.Database.EnsureCreatedAsync();

            var user = await _userHelper.GetUserByEmailAsync("eduardo@gmail.com");
            if (user == null)
            {
                user = new User
                {
                    FirstName = "Eduardo",
                    LastName = "Fernandes",
                    Email = "eduardo@gmail.com",
                    UserName = "eduardo@gmail.com",
                    PhoneNumber = "212343555"
                };

                var result = await _userHelper.AddUserAsync(user, "123456");
                if (result != IdentityResult.Success)
                {
                    throw new InvalidOperationException("Could not create the user in seeder");
                }
            }

            if (!_context.Teams.Any())
            {
                AddTeam("F. C. Porto", "Portugal", "Porto", "Estádio do Dragão", user);
                AddTeam("S. L. Benfica", "Portugal", "Lisbon", "Estádio da Luz", user);
                AddTeam("Sporting C. P.", "Portugal", "Lisbon", "Estádio José Alvalade", user);
                AddTeam("S. C. Braga", "Portugal", "Braga", "Estádio Municipal de Braga", user);
                await _context.SaveChangesAsync();
            }

            if (!_context.Games.Any())
            {
                int homeTeamScore1 = _random.Next(10);
                int awayTeamScore1 = _random.Next(10);
                AddGame(homeTeamScore1, awayTeamScore1, DateTime.Now.AddDays(7), user);

                int homeTeamScore2 = _random.Next(10);
                int awayTeamScore2 = _random.Next(10);
                AddGame(homeTeamScore2, awayTeamScore2, DateTime.Now.AddDays(8), user);

                await _context.SaveChangesAsync();
            }

            if (!_context.Players.Any())
            {
                AddPlayer("Abel Ruiz", 25, "Forward", "S. C. Braga", user);
                AddPlayer("Stephen Eustaquio", 28, "Midfielder", "F. C. Porto", user);
                AddPlayer("António Silva ", 23, "Defender", "S. L. Benfica", user);
                AddPlayer("Franco Israel", 30, "Goalkeeper", "Sporting C. P.", user);
                await _context.SaveChangesAsync();
            }
        }

        private void AddTeam(string name, string country, string city, string stadium, User user)
        {
            _context.Teams.Add(new Team
            {
                Name = name,
                Founded = DateTime.Now.AddDays(1),
                Country = country,
                City = city,
                Stadium = stadium,
                User = user
            });
        }

        private void AddGame(int homeTeamScore, int awayTeamScore, DateTime dateTime, User user)
        {
            // Pick two random team names from the list of existing teams
            string[] teamNames = _context.Teams.Select(t => t.Name).ToArray();
            string randomHomeTeam = teamNames[_random.Next(teamNames.Length)];

            // Keep selecting the away team until it's different from the home team
            string randomAwayTeam;
            do
            {
                randomAwayTeam = teamNames[_random.Next(teamNames.Length)];
            } while (randomAwayTeam == randomHomeTeam);


            _context.Games.Add(new Game
            {
                Date = dateTime,
                HomeTeam = randomHomeTeam,
                AwayTeam = randomAwayTeam,
                HomeTeamScore = homeTeamScore,
                AwayTeamScore = awayTeamScore,
                IsPlayed = false,
                User = user
            });
        }

        private void AddPlayer(string name, int age, string position, string teamName, User user)
        {
            var team = _context.Teams.FirstOrDefault(t => t.Name == teamName);

            if (team != null)
            {
                _context.Players.Add(new Player
                {
                    Name = name,
                    Age = age,
                    Position = position,
                    TeamName = team.Name,
                    User = user
                });
            }
        }
    }
}
