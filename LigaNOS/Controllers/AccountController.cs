using LigaNOS.Data;
using LigaNOS.Data.Entities;
using LigaNOS.Helpers;
using LigaNOS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using User = LigaNOS.Data.Entities.User;

namespace LigaNOS.Controllers
{
    
    public class AccountController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly IMailHelper _mailHelper;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly DataContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IUserHelper userHelper,
            IMailHelper mailHelper,
            RoleManager<IdentityRole> roleManager,
            DataContext context,
            UserManager<User> userManager,
            IConfiguration configuration,
            IWebHostEnvironment hostEnvironment,
            ILogger<AccountController> logger)
        {
            _userHelper = userHelper;
            _mailHelper = mailHelper;
            _roleManager = roleManager;
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
            _hostEnvironment = hostEnvironment;
            _logger = logger;
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
                var user = new Data.Entities.User
                {
                    UserName = model.Username,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName
                };

                if (model.ProfileImage != null)
                {
                    // Calculate the physical path to the directory where images should be stored
                    var imageDirectory = Path.Combine(_hostEnvironment.WebRootPath, "images", "photos");

                    if (!Directory.Exists(imageDirectory))
                    {
                        Directory.CreateDirectory(imageDirectory);
                    }

                    var imageFileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ProfileImage.FileName);
                    var imagePathWithFileName = Path.Combine(imageDirectory, imageFileName);

                    using (var stream = new FileStream(imagePathWithFileName, FileMode.Create))
                    {
                        await model.ProfileImage.CopyToAsync(stream);
                    }

                    user.ProfileImage = imageFileName; // Save the image path in the user's record
                }


                var result = await _userHelper.CreateUserAsync(user, model.Password, model.SelectedRole);

                if (result.Succeeded)
                {
                    // Check if the selected role exists
                    var roleExists = await _roleManager.RoleExistsAsync(model.SelectedRole);
                    if (!roleExists)
                    {
                        // If the role doesn't exist, create the role
                        await _roleManager.CreateAsync(new IdentityRole(model.SelectedRole));
                    }

                    // Add the user to the selected role
                    await _userHelper.AddUserToRoleAsync(user, model.SelectedRole);


                    // Generate email confirmation token
                    var emailConfirmationToken = await _userHelper.GenerateEmailConfirmationTokenAsync(user);

                    // Create confirmation link
                    var confirmationLink = Url.Action("ConfirmEmail", "Account", new
                    {
                        userId = user.Id,
                        token = emailConfirmationToken
                    }, protocol: HttpContext.Request.Scheme);

                    // Send confirmation email
                    string myToken = await _userHelper.GenerateEmailConfirmationTokenAsync(user);
                    string tokenLink = Url.Action("ConfirmEmail", "Account", new
                    {
                        userId = user.Id,
                        token = myToken
                    }, protocol: HttpContext.Request.Scheme);

                    Response response = await _mailHelper.SendEmail(user.Email, "Email confirmation",
                        $"<h1>Email Confirmation</h1>To allow the user, please click in this link: </br></br><a href=\"{tokenLink}\">Confirm Email</a>");

                    if (response.IsSuccess)
                    {
                        TempData["EmailConfirmationMessage"] = "Email confirmation link has been sent to the user's email.";
                        return RedirectToAction("Create", "Account");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Failed to send confirmation email.");
                    }
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
            ViewBag.Roles = new SelectList(await _roleManager.Roles.ToListAsync(), "Name", "Name");
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
                // Check if the user is an admin and if there is only one admin user remaining
                if (await _userManager.IsInRoleAsync(user, "Admin") && await _userHelper.GetAdminUserCountAsync() <= 1)
                {
                    // Set error message
                    TempData["AdminDeleteErrorMessage"] = "Cannot delete the only remaining admin user.";
                    return RedirectToAction("UserList", "Account"); // Redirect to the UserList action
                }

                var loggedInUser = await _userManager.GetUserAsync(User);

                if (await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    _logger.LogInformation("User is in 'Admin' role.");

                    if (loggedInUser.Id == user.Id)
                    {
                        _logger.LogInformation("User is trying to delete themselves as an admin.");

                        TempData["AdminDeleteErrorMessage"] = "You cannot delete yourself as an admin.";
                        return RedirectToAction("UserList", "Account");
                    }
                }

                var deletedUsername = user.UserName; // Store the username before deletion

                var result = await _userHelper.DeleteUserAsync(user);

                if (result.Succeeded)
                {
                    if (await _userManager.IsInRoleAsync(user, "Admin"))
                    {
                        TempData["AdminDeletedMessage"] = $"Admin user '{deletedUsername}' deleted successfully.";
                    }
                    else
                    {
                        TempData["AdminDeletedMessage"] = $"User '{deletedUsername}' deleted successfully.";
                    }

                    return RedirectToAction("UserList", "Account"); // Redirect to the UserList action
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
                    bool hasChanges = user.FirstName != model.FirstName || user.LastName != model.LastName;

                    if (hasChanges)
                    {
                        user.FirstName = model.FirstName;
                        user.LastName = model.LastName;
                        var response = await _userHelper.UpdateUserAsync(user);
                        if (response.Succeeded)
                        {
                            ViewBag.UserMessage = "User updated!";
                            ViewBag.UserMessageColor = "text-success"; // Green color for success
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, response.Errors.FirstOrDefault().Description);
                            ViewBag.UserMessageColor = "text-danger"; // Red color for error
                        }
                    }
                    else
                    {
                        ViewBag.UserMessage = "You need to change user info first!";
                        ViewBag.UserMessageColor = "text-warning"; // Yellow color for info
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

        [HttpPost]
        public async Task<IActionResult> CreateToken([FromBody] LoginViewModel model)
        {
            if (this.ModelState.IsValid)
            {
                var user = await _userHelper.GetUserByEmailAsync(model.Username);
                if (user != null)
                {
                    var result = await _userHelper.ValidatePasswordAsync(
                        user,
                        model.Password);

                    if (result.Succeeded)
                    {
                        var claims = new[]
                        {
                            new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                            new Claim(JwtRegisteredClaimNames.Jti, Guid .NewGuid().ToString())
                        };

                        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Tokens:Key"]));
                        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                        var token = new JwtSecurityToken(
                            _configuration["Tokens:Issuer"],
                            _configuration["Tokens:Audience"],
                            claims,
                            expires: DateTime.UtcNow.AddDays(15),
                            signingCredentials: credentials);
                        var results = new
                        {
                            token = new JwtSecurityTokenHandler().WriteToken(token),
                            expiration = token.ValidTo
                        };

                        return this.Created(string.Empty, results);
                    }
                }
            }

            return BadRequest();
        }

        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrEmpty(token))
            {
                return NotFound();
            }

            var user = await _userHelper.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userHelper.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
            {
                return NotFound();
            }

            return View();
        }

        public IActionResult NotAuthorized()
        {
            return View();
        }
    }
}
