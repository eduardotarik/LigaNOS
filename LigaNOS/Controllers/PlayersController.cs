using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LigaNOS.Data;
using LigaNOS.Data.Entities;
using LigaNOS.Helpers;

namespace LigaNOS.Controllers
{
    public class PlayersController : Controller
    {
        private readonly IPlayerRepository _playerRepository;
        private readonly ITeamRepository _teamRepository;
        private readonly IUserHelper _userHelper;

        public PlayersController(IPlayerRepository playerRepository,
            ITeamRepository teamRepository,
            IUserHelper userHelper)
        {
            _playerRepository = playerRepository;
            _teamRepository = teamRepository;
            _userHelper = userHelper;
        }

        // GET: Players
        public IActionResult Index()
        {
            return View(_playerRepository.GetAll().OrderBy(t => t.TeamName));
        }

        // GET: Players/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var player = await _playerRepository.GetByIdAsync(id.Value);
            if (player == null)
            {
                return NotFound();
            }

            return View(player);
        }

        // GET: Players/Create
        public IActionResult Create()
        {
            // Fetch the teams from the repository and populate the ViewBag.Teams
            var teams = _teamRepository.GetAll().ToList();
            ViewBag.Teams = new SelectList(teams, "Name", "Name");

            return View();
        }
    

        // POST: Players/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Player player)
        {
            if (ModelState.IsValid)
            {
                //TODO: Change to the user that is logged
                player.User = await _userHelper.GetUserByEmailAsync("eduardo@gmail.com");

                await _playerRepository.CreateAsync(player);
                return RedirectToAction(nameof(Index));
            }
            // If the model is not valid, fetch the teams again and populate the ViewBag.Teams
            var teams = _teamRepository.GetAll().ToList();
            ViewBag.Teams = new SelectList(teams, "Name", "Name");

            return View(player);
        }

        // GET: Players/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var player = await _playerRepository.GetByIdAsync(id.Value);
            if (player == null)
            {
                return NotFound();
            }

            // Fetch the teams from the repository and populate the ViewBag.Teams
            var teams = _teamRepository.GetAll().ToList();
            ViewBag.Teams = new SelectList(teams, "Name", "Name");

            return View(player);
        }

        // POST: Players/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Player player)
        {
            if (id != player.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    //TODO: Change to the user that is logged
                    player.User = await _userHelper.GetUserByEmailAsync("eduardo@gmail.com");
                    await _playerRepository.UpdateAsync(player);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (! await _playerRepository.ExistAsync(player.Id))
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

            // If the model is not valid, fetch the teams again and populate the ViewBag.Teams
            var teams = _teamRepository.GetAll().ToList();
            ViewBag.Teams = new SelectList(teams, "Name", "Name");

            return View(player);
        }

        // GET: Players/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var player = await _playerRepository.GetByIdAsync(id.Value);
            if (player == null)
            {
                return NotFound();
            }

            return View(player);
        }

        // POST: Players/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var player = await _playerRepository.GetByIdAsync(id);
            await _playerRepository.DeleteAsync(player);
            return RedirectToAction(nameof(Index));
        }
    }
}
