using LigaNOS.Data;
using LigaNOS.Helpers;
using LigaNOS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LigaNOS.Controllers
{
    [RoleAuthorization("Admin")]
    public class TeamsController : Controller
    {
        private readonly ITeamRepository _teamRepository;
        private readonly IBlobHelper _blobHelper;
        private readonly IConverterHelper _converterHelper;
        private readonly IUserHelper _userHelper;

        public TeamsController(ITeamRepository teamRepository,
            IBlobHelper blobHelper,
            IConverterHelper converterHelper,
            IUserHelper userHelper)
        {
            _teamRepository = teamRepository;
            _blobHelper = blobHelper;
            _converterHelper = converterHelper;
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
        [Authorize(Roles = "Admin")]
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
                Guid imageId = Guid.Empty;

                if (model.ImageFile != null && model.ImageFile.Length > 0)
                {
                    imageId = await _blobHelper.UploadBlobAsync(model.ImageFile, "emblems");
                }

                var team = _converterHelper.ToTeam(model, imageId, true);

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

            var model = _converterHelper.ToTeamViewModel(team);

            return View(model);
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
                    Guid imageId = model.ImageId;

                    if (model.ImageFile != null && model.ImageFile.Length > 0)
                    {
                        imageId = await _blobHelper.UploadBlobAsync(model.ImageFile, "emblems");
                    }

                    var team = _converterHelper.ToTeam(model, imageId, false);

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
