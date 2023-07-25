using LigaNOS.Data.Entities;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Bogus;
using System.Collections.Generic;

namespace LigaNOS.Data
{
    public class SeedDb
    {
        private readonly DataContext _context;
        private Random _random;
        private readonly Faker _faker;
        private List<Team> _teams;

        public SeedDb(DataContext context)
        {
            _context = context;
            _random = new Random();
            _faker = new Faker();
            _teams = new List<Team>();
        }

        public async Task SeedAsync()
        {
            await _context.Database.EnsureCreatedAsync();

            if (!_context.Teams.Any())
            {
                AddTeam("F. C. Porto", "Portugal", "Porto", "Estádio do Dragão");
                AddTeam("S. L. Benfica", "Portugal", "Lisbon", "Estádio da Luz");
                AddTeam("Sporting C. P.", "Portugal", "Lisbon", "Estádio José Alvalade");
                AddTeam("S. C. Braga", "Portugal", "Braga", "Estádio Municipal de Braga");
                await _context.SaveChangesAsync();
            }

            _teams = _context.Teams.ToList();

            if (!_context.Games.Any())
            {
                int homeTeamScore1 = 2;
                int awayTeamScore1 = 1;
                AddGame("S. C. Braga", "S. L. Benfica", homeTeamScore1, awayTeamScore1, DateTime.Now.AddDays(7));

                int homeTeamScore2 = 0;
                int awayTeamScore2 = 0;
                AddGame("F. C. Porto", "Sporting C. P.", homeTeamScore2, awayTeamScore2, DateTime.Now.AddDays(8));

                await _context.SaveChangesAsync();
            }

            if (!_context.Players.Any())
            {
                AddPlayer("Abel Ruiz", 25, "Forward", "S. C. Braga");
                AddPlayer("Stephen Eustaquio", 28, "Midfielder", "F. C. Porto");
                AddPlayer("António Silva ", 23, "Defender", "S. L. Benfica");
                AddPlayer("Franco Israel", 30, "Goalkeeper", "Sporting C. P.");
                await _context.SaveChangesAsync();
            }
        }

        private void AddTeam(string name, string country, string city, string stadium)
        {
            _context.Teams.Add(new Team
            {
                Name = name,
                Founded = DateTime.Now.AddDays(1),
                Country = country,
                City = city,
                Stadium = stadium
            });
        }

        private void AddGame(string homeTeam, string awayTeam, int homeTeamScore, int awayTeamScore, DateTime dateTime)
        {
            _context.Games.Add(new Game
            {
                Date = dateTime,
                HomeTeam = homeTeam,
                AwayTeam = awayTeam,
                HomeTeamScore = homeTeamScore,
                AwayTeamScore = awayTeamScore,
                IsPlayed = false,
            });
        }

        private void AddPlayer(string name, int age, string position, string teamName)
        {
            var team = _context.Teams.FirstOrDefault(t => t.Name == teamName);

            if (team != null)
            {
                _context.Players.Add(new Player
                {
                    Name = name,
                    Age = age,
                    Position = position,
                    TeamName = team.Name
                });
            }
        }
    }
}
