using LigaNOS.Data;
using LigaNOS.Helpers;
using LigaNOS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LigaNOS.Controllers
{
    public class PlayersController : Controller
    {
        private readonly IPlayerRepository _playerRepository;
        private readonly ITeamRepository _teamRepository;
        private readonly IBlobHelper _blobHelper;
        private readonly IConverterHelper _converterHelper;
        private readonly IUserHelper _userHelper;

        public PlayersController(IPlayerRepository playerRepository,
            ITeamRepository teamRepository,
            IBlobHelper blobHelper,
            IConverterHelper converterHelper,
            IUserHelper userHelper)
        {
            _playerRepository = playerRepository;
            _teamRepository = teamRepository;
            _blobHelper = blobHelper;
            _converterHelper = converterHelper;
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
                return View("PlayerNotFound");
            }

            var player = await _playerRepository.GetByIdAsync(id.Value);
            if (player == null)
            {
                return View("PlayerNotFound");
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
        public async Task<IActionResult> Create(PlayerViewModel model)
        {
            if (ModelState.IsValid)
            {
                Guid pictureId = Guid.Empty;

                if (model.ImageFile != null && model.ImageFile.Length > 0)
                {
                    pictureId = await _blobHelper.UploadBlobAsync(model.ImageFile, "pictures");
                }

                var player = _converterHelper.ToPlayer(model, pictureId, true);

                //TODO: Change to the user that is logged
                player.User = await _userHelper.GetUserByEmailAsync(this.User.Identity.Name);
                await _playerRepository.CreateAsync(player);
                return RedirectToAction(nameof(Index));
            }
            // If the model is not valid, fetch the teams again and populate the ViewBag.Teams
            var teams = _teamRepository.GetAll().ToList();
            ViewBag.Teams = new SelectList(teams, "Name", "Name");

            return View(model);
        }

        // GET: Players/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return View("PlayerNotFound");
            }

            var player = await _playerRepository.GetByIdAsync(id.Value);
            if (player == null)
            {
                return View("PlayerNotFound");
            }

            var model = _converterHelper.ToPlayerViewModel(player);

            // Fetch the teams from the repository and populate the ViewBag.Teams
            var teams = _teamRepository.GetAll().ToList();
            ViewBag.Teams = new SelectList(teams, "Name", "Name");

            return View(model);
        }

        // POST: Players/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PlayerViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    Guid pictureId = model.PictureId;

                    if (model.ImageFile != null && model.ImageFile.Length > 0)
                    {
                        pictureId = await _blobHelper.UploadBlobAsync(model.ImageFile, "pictures");
                    }

                    var player = _converterHelper.ToPlayer(model, pictureId, false);

                    //TODO: Change to the user that is logged
                    player.User = await _userHelper.GetUserByEmailAsync(this.User.Identity.Name);
                    await _playerRepository.UpdateAsync(player);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _playerRepository.ExistAsync(model.Id))
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

            return View(model);
        }

        // GET: Players/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return View("PlayerNotFound");
            }

            var player = await _playerRepository.GetByIdAsync(id.Value);
            if (player == null)
            {
                return View("PlayerNotFound");
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

        public IActionResult PlayerNotFound()
        {
            return View();
        }
    }
}
