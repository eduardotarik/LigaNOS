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

namespace LigaNOS.Controllers
{
    public class GamesController : Controller
    {
        private readonly IGameRepository _gameRepository;

        public GamesController(IGameRepository gameRepository)
        {
            _gameRepository = gameRepository;
        }

        // GET: Games
        public IActionResult Index()
        {
            return View(_gameRepository.GetAll());
        }

        // GET: Games/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var game = await _gameRepository.GetByIdAsync(id.Value);
            if (game == null)
            {
                return NotFound();
            }

            return View(game);
        }

        // GET: Games/Create
        public IActionResult Create()
        {
            return View();
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
                await _gameRepository.CreateAsync(game);
                return RedirectToAction(nameof(Index));
            }
            return View(game);
        }

        // GET: Games/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var game = await _gameRepository.GetByIdAsync(id.Value);
            if (game == null)
            {
                return NotFound();
            }
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
                    _gameRepository.UpdateAsync(game);
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
                return NotFound();
            }

            var game = await _gameRepository.GetByIdAsync(id.Value);
            if (game == null)
            {
                return NotFound();
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

            if (games == null || games.Count == 0)
            {
                return View("Error");
            }

            var statistics = CalculateStatistics(games);

            return View(statistics);
        }

        private List<TeamStatisticsViewModel> CalculateStatistics(List<Game> games)
        {
            var teamStats = new Dictionary<string, TeamStatisticsViewModel>();

            foreach (var game in games)
            {
                UpdateTeamStatistics(teamStats, game.HomeTeam, game.HomeTeamScore, game.AwayTeamScore);
                UpdateTeamStatistics(teamStats, game.AwayTeam, game.AwayTeamScore, game.HomeTeamScore);
            }

            // Sort the team statistics by points in descending order
            var sortedTeamStats = teamStats.Values.OrderByDescending(stats => stats.Points).ToList();

            return sortedTeamStats;
        }

        private void UpdateTeamStatistics(Dictionary<string, TeamStatisticsViewModel> teamStats, string teamName, int? scoredGoals, int? concededGoals)
        {
            if (!teamStats.ContainsKey(teamName))
            {
                teamStats[teamName] = new TeamStatisticsViewModel { TeamName = teamName };
            }

            var stats = teamStats[teamName];

            stats.TotalGames++;
            stats.GoalsFor += scoredGoals ?? 0;
            stats.GoalsAgainst += concededGoals ?? 0;

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
