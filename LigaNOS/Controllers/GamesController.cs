using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LigaNOS.Data;
using LigaNOS.Data.Entities;
using LigaNOS.Models;
using LigaNOS.Helpers;
using Microsoft.AspNetCore.Authorization;
using User = Microsoft.SqlServer.Management.Smo.User;
using static LigaNOS.Data.Entities.Game;

namespace LigaNOS.Controllers
{
    public class GamesController : Controller
    {
        private readonly IGameRepository _gameRepository;
        private readonly ITeamRepository _teamRepository;
        private readonly IUserHelper _userHelper;

        public GamesController(IGameRepository gameRepository,
            ITeamRepository teamRepository,
            IUserHelper userHelper)
        {
            _gameRepository = gameRepository;
            _teamRepository = teamRepository;
            _userHelper = userHelper;
        }

        // GET: Games
        public IActionResult Index()
        {
            var games = _gameRepository.GetAll().OrderBy(d => d.Date).ToList();
            return View(games);
        }

        // GET: Games/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return View("GameNotFound");
            }

            var game = await _gameRepository.GetByIdAsync(id.Value);
            if (game == null)
            {
                return View("GameNotFound");
            }

            return View(game);
        }

        // GET: Games/Create
        public IActionResult Create()
        {
            var newGame = new Game();
            newGame.Date = DateTime.Today; // Set a default date for testing

            var teams = _teamRepository.GetAll().ToList();
            ViewBag.Teams = new SelectList(teams, "Name", "Name");
            

            return View(newGame);
        }

        // POST: Games/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Game game)
        {
            if (ModelState.IsValid)
            {
                //TODO: Change to the user that is logged
                game.User = await _userHelper.GetUserByEmailAsync(this.User.Identity.Name);

                await _gameRepository.CreateAsync(game);
                return RedirectToAction(nameof(Index));
            }
            ViewBag.TeamNames = new SelectList(GetTeamNames());
            return View(game);
        }


        // GET: Games/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return View("GameNotFound");
            }

            var game = await _gameRepository.GetByIdAsync(id.Value);
            if (game == null)
            {
                return View("GameNotFound");
            }

            // Fetch the teams from the repository and populate the ViewBag.Teams
            var teams = _teamRepository.GetAll().ToList();
            ViewBag.Teams = new SelectList(teams, "Name", "Name", game.HomeTeam); // Pass the selected home team name
            ViewBag.AwayTeams = new SelectList(teams, "Name", "Name", game.AwayTeam); // Pass the selected away team name

            return View(game);
        }

        // POST: Games/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Game game)
        {
            if (id != game.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    //TODO: Change to the user that is logged
                    game.User = await _userHelper.GetUserByEmailAsync(this.User.Identity.Name);
                    await _gameRepository.UpdateAsync(game);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (! await _gameRepository.ExistAsync(game.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(game);
        }

        // GET: Games/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return View("GameNotFound");
            }

            var game = await _gameRepository.GetByIdAsync(id.Value);
            if (game == null)
            {
                return View("GameNotFound");
            }

            return View(game);
        }

        // POST: Games/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var game = await _gameRepository.GetByIdAsync(id);
            await _gameRepository.DeleteAsync(game);
            return RedirectToAction(nameof(Index));
        }

        // GET: Games/Statistics
        public IActionResult Statistics()
        {
            var games = _gameRepository.GetAll().ToList();

            var statistics = CalculateStatistics(games);

            return View(statistics);
        }

        private List<TeamStatisticsViewModel> CalculateStatistics(List<Game> games)
        {
            var teamStats = new Dictionary<string, TeamStatisticsViewModel>();

            foreach (var game in games)
            {
                if (game.IsPlayed) // Consider statistics only for played games
                {
                    UpdateTeamStatistics(teamStats, game.HomeTeam, game.HomeTeamScore, game.AwayTeamScore, game.IsPlayed);
                    UpdateTeamStatistics(teamStats, game.AwayTeam, game.AwayTeamScore, game.HomeTeamScore, game.IsPlayed);
                }
            }

            // Sort the team statistics by points in descending order
            var sortedTeamStats = teamStats.Values.OrderByDescending(stats => stats.Points).ToList();

            return sortedTeamStats;
        }

        private void UpdateTeamStatistics(Dictionary<string, TeamStatisticsViewModel> teamStats, string teamName, int? scoredGoals, int? concededGoals, bool isPlayed)
        {
            if (!teamStats.ContainsKey(teamName))
            {
                teamStats[teamName] = new TeamStatisticsViewModel { TeamName = teamName };
            }

            var stats = teamStats[teamName];

            stats.TotalGames++;

            if (isPlayed)
            {
                // Check if both scoredGoals and concededGoals are not null before updating
                if (scoredGoals.HasValue && concededGoals.HasValue)
                {
                    stats.GoalsFor += scoredGoals.Value;
                    stats.GoalsAgainst += concededGoals.Value;

                    if (scoredGoals > concededGoals)
                    {
                        stats.Wins++;
                    }
                    else if (scoredGoals < concededGoals)
                    {
                        stats.Losses++;
                    }
                    else
                    {
                        stats.Draws++;
                    }
                } 
            }
        }

        private List<string> GetTeamNames()
        {
            var teamNames = _gameRepository.GetAll().Select(g => g.HomeTeam).Distinct().ToList();
            teamNames.AddRange(_gameRepository.GetAll().Select(g => g.AwayTeam).Distinct().ToList());
            return teamNames.Distinct().ToList();
        }

        [HttpPost]
        public async Task<IActionResult> IssueCard(int gameId, string cardType, string team)
        {
            var game = await _gameRepository.GetByIdAsync(gameId);

            if (game == null)
            {
                // Game not found, handle error
                return NotFound();
            }

            if (team == "Home")
            {
                if (cardType == "Red")
                {
                    game.HomeTeamIssuedCard = "Red";
                }
                else if (cardType == "Yellow")
                {
                    game.HomeTeamIssuedCard = "Yellow";
                }
            }
            else if (team == "Away")
            {
                if (cardType == "Red")
                {
                    game.AwayTeamIssuedCard = "Red";
                }
                else if (cardType == "Yellow")
                {
                    game.AwayTeamIssuedCard = "Yellow";
                }
            }

            await _gameRepository.UpdateAsync(game);

            return RedirectToAction("Index");
        }

        public IActionResult GameNotFound()
        {
            return View();
        }

        public async Task<IActionResult> GenerateSeasonGames()
        {
            var teams = _teamRepository.GetAll().ToList();

            // Clear existing games
            await _gameRepository.ClearGamesAsync();

            var random = new Random();
            var startHour = 6; // Start at 6:00 AM
            var endHour = 9;   // End at 9:00 AM the next day
            var intervalMinutes = 30; // 30-minute intervals

            // Generate games
            foreach (var homeTeam in teams)
            {
                foreach (var awayTeam in teams)
                {
                    if (homeTeam.Id != awayTeam.Id)
                    {
                        // Calculate a random date within the range
                        var daysToAdd = random.Next(1, 30);
                        var hoursToAdd = random.Next(startHour, endHour);
                        var minutesToAdd = random.Next(0, 30) * intervalMinutes;

                        var gameDate = DateTime.Today
                            .AddDays(daysToAdd)
                            .AddHours(hoursToAdd)
                            .AddMinutes(minutesToAdd);

                        var game = new Game
                        {
                            HomeTeam = homeTeam.Name,
                            AwayTeam = awayTeam.Name,
                            Date = gameDate,
                            HomeTeamScore = null, // Set a default score
                            AwayTeamScore = null, // Set a default score
                            IsPlayed = false,
                        };

                        await _gameRepository.CreateAsync(game);
                    }
                }
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> StartSeason()
        {
            // Generate the season's games
            await GenerateSeasonGames();

            var games = await _gameRepository.GetAllAsync(); // Retrieve all games

            foreach (var game in games)
            {
                game.Status = Game.SeasonStatus.Active; // Set the status to Active
            }

            await _gameRepository.SaveAllAsync(); // Save changes to the database

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> EndSeason()
        {
            await _gameRepository.ClearGamesAsync();

            var games = await _gameRepository.GetAllAsync();

            foreach (var game in games)
            {
                game.Status = Game.SeasonStatus.Ended;
                game.Stat = Game.GameStatus.Ended;
            }

            await _gameRepository.SaveAllAsync();

            return RedirectToAction(nameof(Index));
        }

        // POST: Games/ConfirmEndSeason
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmEndSeason()
        {
            // Clear existing games before marking them as ended
            await _gameRepository.ClearGamesAsync();

            var games = await _gameRepository.GetAllAsync();

            foreach (var game in games)
            {
                game.Status = Game.SeasonStatus.Ended;
                game.Stat = Game.GameStatus.Ended;
            }

            await _gameRepository.SaveAllAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
