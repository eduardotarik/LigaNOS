using LigaNOS.Data;
using LigaNOS.Data.Entities;
using LigaNOS.Helpers;
using LigaNOS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using User = LigaNOS.Data.Entities.User;

namespace LigaNOS.Controllers
{
    
    public class AccountController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly DataContext _context;
        private readonly UserManager<User> _userManager;

        public AccountController(IUserHelper userHelper,
            RoleManager<IdentityRole> roleManager,
            DataContext context,
            UserManager<User> userManager)
        {
            _userHelper = userHelper;
            _roleManager = roleManager;
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userHelper.GetUserByEmailAsync(model.Username);

                if (user != null && await _userManager.IsInRoleAsync(user, "Admin") ||
                                    await _userManager.IsInRoleAsync(user, "Team") ||
                                    await _userManager.IsInRoleAsync(user, "Staff"))
                {
                    var result = await _userHelper.LoginAsync(model);
                    if (result.Succeeded)
                    {
                        if (this.Request.Query.Keys.Contains("ReturnUrl"))
                        {
                            return Redirect(this.Request.Query["ReturnUrl"].First());
                        }

                        return RedirectToAction("Index", "Home");
                    }
                }
            }

            this.ModelState.AddModelError(string.Empty, "Failed to login");
            return View(model);
        }


        public async Task<IActionResult> Logout()
        {
            await _userHelper.LogoutAsync();
            return RedirectToAction("Index", "Home");
        }

        // Add a new action to display the account creation form
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            var roles = _roleManager.Roles.ToList();
            ViewBag.Roles = new SelectList(roles, "Name", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorization("Admin")]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.Password != model.ConfirmPassword)
                {
                    ModelState.AddModelError("", "The password and confirmation password do not match.");
                    var rolesList = await _roleManager.Roles.ToListAsync();
                    ViewBag.Roles = new SelectList(rolesList, "Name", "Name");
                    return View(model);
                }

                var user = new Data.Entities.User
                {
                    UserName = model.Username,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName
                };

                var result = await _userHelper.CreateUserAsync(user, model.Password, model.SelectedRole);

                if (result.Succeeded)
                {
                    // User created successfully
                    // Redirect or show a success message
                    return RedirectToAction("Index", "Home"); // Redirect to the desired page
                }
                else
                {
                    // Handle user creation failure
                    // Show error messages
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }

            // Repopulate the roles dropdown in case of validation errors
            var roles = await _roleManager.Roles.ToListAsync();
            ViewBag.Roles = new SelectList(roles, "Name", "Name");
            return View(model);
        }

        [RoleAuthorization("Admin")]
        public async Task<IActionResult> Delete(string userId)
        {
            var user = await _userHelper.GetUserByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorization("Admin")]
        public async Task<IActionResult> DeleteConfirmed(string Id)
        {
            var user = await _userHelper.GetUserByIdAsync(Id);

            if (user != null)
            {
                var result = await _userHelper.DeleteUserAsync(user);

                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home"); // Redirect to the desired page
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }
            else
            {
                ModelState.AddModelError("", "User not found.");
            }

            // Redirect back to the Delete view with the user's details
            return View("Delete", user);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UserList()
        {
            var users = await _userHelper.GetAllUsersAsync();

            // Create a URL that includes the current URL as a query parameter
            var currentUrl = Url.Action("UserList", "Account", null, Request.Scheme);
            var usersWithRoles = new List<UserWithRolesViewModel>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                usersWithRoles.Add(new UserWithRolesViewModel
                {
                    User = user,
                    Roles = roles.ToList(),
                    UserListUrl = currentUrl // Pass the URL to the view model
                });
            }

            return View(usersWithRoles);
        }

        public async Task<IActionResult> ChangeUser()
        {
            var user = await _userHelper.GetUserByEmailAsync(this.User.Identity.Name);
            var model = new ChangeUserViewModel();
            if (user != null)
            {
                model.FirstName = user.FirstName;
                model.LastName = user.LastName;
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ChangeUser(ChangeUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userHelper.GetUserByEmailAsync(this.User.Identity.Name);
                if (user != null)
                {
                    user.FirstName = model.FirstName;
                    user.LastName = model.LastName;
                    var response = await _userHelper.UpdateUserAsync(user);
                    if (response.Succeeded)
                    {
                        ViewBag.UserMessage = "User updated!";
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, response.Errors.FirstOrDefault().Description);
                    }
                }
            }

            return View(model);
        }

        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userHelper.GetUserByEmailAsync(this.User.Identity.Name);
                if (user != null)
                {
                    var result = await _userHelper.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
                    if (result.Succeeded)
                    {
                        return this.RedirectToAction("ChangeUser");
                    }
                    else
                    {
                        this.ModelState.AddModelError(string.Empty, result.Errors.FirstOrDefault().Description);
                    }
                }
                else
                {
                    this.ModelState.AddModelError(string.Empty, "User not found.");
                }
            }

            return this.View(model);
        }
    }
}
