using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LigaNOS.Data;
using LigaNOS.Data.Entities;
using System.Xml.Schema;
using LigaNOS.Helpers;
using LigaNOS.Models;
using System.IO;

namespace LigaNOS.Controllers
{
    public class TeamsController : Controller
    {
        private readonly ITeamRepository _teamRepository;
        private readonly IUserHelper _userHelper;

        public TeamsController(ITeamRepository teamRepository,
            IUserHelper userHelper)
        {
            _teamRepository = teamRepository;
            _userHelper = userHelper;
        }

        // GET: Teams
        public IActionResult Index()
        {
            return View(_teamRepository.GetAll().OrderBy(t => t.Name));
        }

        // GET: Teams/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var team = await _teamRepository.GetByIdAsync(id.Value);
            if (team == null)
            {
                return NotFound();
            }

            return View(team);
        }

        // GET: Teams/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Teams/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TeamViewModel model)
        {
            if (ModelState.IsValid)
            {
                var path = string.Empty;

                if (model.ImageFile != null && model.ImageFile.Length > 0)
                {
                    var guid = Guid.NewGuid().ToString();
                    var file = $"{guid}.jpg";

                    path = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot\\images\\emblems",
                        file);

                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await model.ImageFile.CopyToAsync(stream);
                    }

                    path = $"~/images/emblems/{file}";
                }

                var team = this.ToTeam(model, path);

                // Check for duplicate team name
                var existingTeam = await _teamRepository.GetByNameAsync(team.Name);
                if (existingTeam != null)
                {
                    ModelState.AddModelError("Name", "A team with the same name already exists.");
                    return View(model); // Return the view with the validation error message
                }

                //TODO: Change to the user that is logged
                team.User = await _userHelper.GetUserByEmailAsync("eduardo@gmail.com");

                await _teamRepository.CreateAsync(team);
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        private Team ToTeam(TeamViewModel model, string path)
        {
            return new Team
            {
                Id = model.Id,
                Emblem = path,
                Name = model.Name,
                Founded = model.Founded,
                Country = model.Country,
                City = model.City,
                Stadium = model.Stadium,
                User = model.User,
            };
        }

        // GET: Teams/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var team = await _teamRepository.GetByIdAsync(id.Value);
            if (team == null)
            {
                return NotFound();
            }

            var model = this.ToTeamViewModel(team);

            return View(model);
        }

        private TeamViewModel ToTeamViewModel(Team team)
        {
            return new TeamViewModel
            {
                Id = team.Id,
                Emblem = team.Emblem,
                Name = team.Name,
                Founded = team.Founded,
                Country = team.Country,
                City = team.City,
                Stadium = team.Stadium,
                User = team.User
            };
        }

        // POST: Teams/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TeamViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check for duplicate team name
                var existingTeam = await _teamRepository.GetByNameAsync(model.Name);
                if (existingTeam != null && existingTeam.Id != model.Id)
                {
                    ModelState.AddModelError("Name", "A team with the same name already exists.");

                    // Fetch the teams again and populate the ViewBag.Teams
                    var teams = _teamRepository.GetAll().ToList();
                    ViewBag.Teams = new SelectList(teams, "Name", "Name");

                    return View(model);
                }

                try
                {
                    var path = model.Emblem;

                    if (model.ImageFile != null && model.ImageFile.Length > 0)
                    {
                        var guid = Guid.NewGuid().ToString();
                        var file = $"{guid}.jpg";

                        path = Path.Combine(
                            Directory.GetCurrentDirectory(),
                            "wwwroot\\images\\emblems",
                            file);

                        using (var stream = new FileStream(path, FileMode.Create))
                        {
                            await model.ImageFile.CopyToAsync(stream);
                        }

                        path = $"~/images/emblems/{file}";
                    }

                    var team = this.ToTeam(model, path);

                    //TODO: Change to the user that is logged
                    team.User = await _userHelper.GetUserByEmailAsync("eduardo@gmail.com");
                    await _teamRepository.UpdateAsync(team);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _teamRepository.ExistAsync(model.Id))
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
            var teamsList = _teamRepository.GetAll().ToList();
            ViewBag.Teams = new SelectList(teamsList, "Name", "Name", model.Name);

            return View(model);
        }

        // GET: Teams/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var team = await _teamRepository.GetByIdAsync(id.Value);
            if (team == null)
            {
                return NotFound();
            }

            return View(team);
        }

        // POST: Teams/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var team = await _teamRepository.GetByIdAsync(id);
            await _teamRepository.DeleteAsync(team);
            return RedirectToAction(nameof(Index));
        }

    }
}
