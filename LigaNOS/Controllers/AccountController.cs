using LigaNOS.Data;
using LigaNOS.Data.Entities;
using LigaNOS.Helpers;
using LigaNOS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
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
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IUserHelper userHelper,
            IMailHelper mailHelper,
            RoleManager<IdentityRole> roleManager,
            DataContext context,
            UserManager<User> userManager,
            IConfiguration configuration,
            IWebHostEnvironment hostingEnvironment,
            ILogger<AccountController> logger)
        {
            _userHelper = userHelper;
            _mailHelper = mailHelper;
            _roleManager = roleManager;
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
            _hostingEnvironment = hostingEnvironment;
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

                if (user != null)
                {
                    if (await _userManager.IsInRoleAsync(user, "Admin") ||
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
                        else
                        {
                            // Handle login failure if needed.
                            ModelState.AddModelError(string.Empty, "Failed to login");
                        }
                    }
                    else
                    {
                        // Handle the case where the user is not in any of the specified roles.
                        ModelState.AddModelError(string.Empty, "User is not authorized.");
                    }
                }
                else
                {
                    // Handle the case where the user with the given email address was not found.
                    ModelState.AddModelError(string.Empty, "User not found.");
                }
            }

            // If any validation error occurs, return to the login view with error messages.
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
                    var imageDirectory = Path.Combine(_hostingEnvironment.WebRootPath, "images", "photos");

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

                    // Create confirmation link with the custom redirectToReset parameter
                    var recoverPasswordUrl = Url.Action("RecoverPassword", "Account", new
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

        private bool IsDefaultAdmin(User user)
        {
            return user.UserName == "eduardo@gmail.com";
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorization("Admin")]
        public async Task<IActionResult> DeleteConfirmed(string Id)
        {
            var user = await _userHelper.GetUserByIdAsync(Id);

            bool isDefaultAdmin = IsDefaultAdmin(user);

            if (isDefaultAdmin)
            {
                // Set an error message indicating that the default admin cannot be deleted
                TempData["AdminDeleteErrorMessage"] = "The default admin cannot be deleted.";
                return RedirectToAction("UserList", "Account"); // Redirect to the UserList action
            }

            // Check for associated records in the "Games" table (replace "SomeUserIdProperty" with the actual property name)
            bool gamesAssociatedRecords = _context.Games.Any(g => g.UserId == user.Id);

            bool playersAssociatedRecords = _context.Players.Any(p => p.UserId == user.Id);

            bool teamsAssociatedRecords = _context.Teams.Any(t => t.UserId == user.Id);


            // Continue with the rest of your code...

            if (user != null)
            {
                var loggedInUser = await _userManager.GetUserAsync(User);

                // Check if the user is an admin and if there is only one admin user remaining
                if (await _userManager.IsInRoleAsync(user, "Admin") && await _userHelper.GetAdminUserCountAsync() <= 1)
                {
                    // Set error message
                    TempData["AdminDeleteErrorMessage"] = "Cannot delete the only remaining admin user.";
                    return RedirectToAction("UserList", "Account"); // Redirect to the UserList action
                }

                if (loggedInUser.Id == user.Id)
                {
                    // Set error message if the user is trying to delete themselves as an admin
                    TempData["AdminDeleteErrorMessage"] = "You cannot delete yourself as an admin.";
                    return RedirectToAction("UserList", "Account");
                }

                var deletedUsername = user.UserName; // Store the username before deletion

                if (gamesAssociatedRecords || playersAssociatedRecords || teamsAssociatedRecords)
                {
                    // Set an error message indicating that the user has associated records and cannot be deleted
                    TempData["AdminDeleteErrorMessage"] = $"Cannot delete user '{deletedUsername}' because they have associated records.";
                    return RedirectToAction("UserList", "Account"); // Redirect to the UserList action or another appropriate action
                }

                // Here, you can delete the user
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



        // Check if the user is the default admin user based on their username
        private bool IsDefaultAdminUser(User user)
        {
            // You can modify this condition to match the username of your default admin user
            return _userManager.IsInRoleAsync(user, "DefaultAdminRoleName").Result;
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
                    UserListUrl = currentUrl // URL to the view model
                });
            }

            return View(usersWithRoles);
        }

        [RoleAuthorization("Admin", "Team", "Staff")]
        [HttpGet]
        public async Task<IActionResult> ChangeUser()
        {
            if (TempData.ContainsKey("SuccessMessage"))
            {
                ViewBag.SuccessMessage = TempData["SuccessMessage"];
            }

            if (TempData.ContainsKey("ErrorMessage"))
            {
                ViewBag.ErrorMessage = TempData["ErrorMessage"];
            }

            var user = await _userHelper.GetUserByEmailAsync(this.User.Identity.Name);
            var model = new ChangeUserViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                CurrentProfileImage = user.ProfileImage // Populate the CurrentProfileImage property
            };

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
                    // Check if a new image was uploaded
                    if (model.ProfileImage != null)
                    {
                        // Process the new image, e.g., save it to the server
                        var imageFileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ProfileImage.FileName);
                        var imagePathWithFileName = Path.Combine(_hostingEnvironment.WebRootPath, "images", "photos", imageFileName);

                        using (var stream = new FileStream(imagePathWithFileName, FileMode.Create))
                        {
                            await model.ProfileImage.CopyToAsync(stream);
                        }

                        // Update the user's ProfileImage property with the new image file name
                        user.ProfileImage = imageFileName;
                    }

                    // Update other user properties
                    user.FirstName = model.FirstName;
                    user.LastName = model.LastName;

                    // Update the user in the database
                    var result = await _userHelper.UpdateUserAsync(user);

                    if (result.Succeeded)
                    {
                        // Success message in the view model
                        model.UserUpdatedMessage = "User information updated successfully. Click on your username to see the changes!";

                        // Return the updated model to display the updated information
                        return View(model);
                    }
                    else
                    {
                        // Handle errors, e.g., result.Errors
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "User not found.");
                }
            }

            // Repopulate the form and display errors
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
                        TempData["SuccessMessage"] = "Password updated successfully.";
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }

                        TempData["ErrorMessage"] = "Password update failed.";
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "User not found.");
                }
            }

            return RedirectToAction("ChangeUser");
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

            // Generate a password reset token for the user
            var resetPasswordToken = await _userHelper.GeneratePasswordResetTokenAsync(user);

            // Redirect the user to the ResetPassword action with the user's ID and reset password token as query parameters
            return RedirectToAction("ResetPassword", new { userId = user.Id, token = resetPasswordToken });
        }

        public IActionResult NotAuthorized()
        {
            return View();
        }

        [HttpGet]
        [RoleAuthorization("Admin")] // Ensure that only Admins can access this action
        public async Task<IActionResult> Edit(string id)
        {
            // Retrieve the user by ID
            var user = await _userHelper.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Map the user information to an EditUserViewModel (create this ViewModel)
            var editUserViewModel = new EditUserViewModel
            {
                Id = user.Id,
                Username = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                // Add other properties you want to edit
            };

            // Pass the user to the view
            return View(editUserViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorization("Admin")] // Ensure that only Admins can access this action
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userHelper.GetUserByIdAsync(model.Id);

                if (user != null)
                {
                    // Check if any updates were made
                    if (!string.Equals(user.UserName, model.Username) || !string.Equals(user.Email, model.Email) || !string.Equals(user.FirstName, model.FirstName) || !string.Equals(user.LastName, model.LastName))
                    {
                        // Handle updates here

                        // Set a success message in TempData
                        TempData["UserUpdatedMessage"] = "User information updated successfully.";
                    }
                    else
                    {
                        TempData["UserUpdatedMessage"] = null; // Clear the message if no updates were made
                    }

                    // Check if a new image was uploaded
                    if (model.ProfileImage != null)
                    {
                        // Process the new image, e.g., save it to the server
                        string newImageFileName = await ProcessUploadedFileAsync(model.ProfileImage);

                        // Delete the old image if necessary
                        if (!string.IsNullOrEmpty(user.ProfileImage))
                        {
                            DeleteOldImage(user.ProfileImage);
                        }

                        // Update the user's ProfileImage property with the new image file name
                        user.ProfileImage = newImageFileName;
                    }

                    // Update other user properties
                    user.UserName = model.Username;
                    user.Email = model.Email;
                    user.FirstName = model.FirstName;
                    user.LastName = model.LastName;

                    // Update the user in the database
                    var result = await _userHelper.UpdateUserAsync(user);

                    if (result.Succeeded)
                    {
                        TempData["UserUpdatedMessage"] = "User information updated successfully.";
                        return RedirectToAction("UserList");
                    }
                    else
                    {
                        // Handle errors, e.g., result.Errors
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "User not found.");
                }
            }

            // Repopulate the form and display errors
            return View(model);
        }


        private async Task<string> ProcessUploadedFileAsync(IFormFile profileImage)
        {
            if (profileImage == null || profileImage.Length == 0)
            {
                return null; // No file uploaded or file size is zero
            }

            // Generate a unique file name for the uploaded image
            string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(profileImage.FileName);
            string uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "images", "photos");

            // Ensure the directory exists
            Directory.CreateDirectory(uploadsFolder);

            // Combine the uploads folder and the unique file name to get the full file path
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // Save the uploaded image to the file system
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await profileImage.CopyToAsync(stream);
            }

            return uniqueFileName;
        }

        private void DeleteOldImage(string fileName)
        {
            string filePath = Path.Combine(_hostingEnvironment.WebRootPath, "images", "photos", fileName);

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadProfileImage(IFormFile profileImage)
        {
            if (profileImage != null && profileImage.Length > 0)
            {
                var user = await _userHelper.GetUserByEmailAsync(this.User.Identity.Name);

                if (user != null)
                {
                    // Process the new image, e.g., save it to the server
                    var imageFileName = Guid.NewGuid().ToString() + Path.GetExtension(profileImage.FileName);
                    var imagePathWithFileName = Path.Combine(_hostingEnvironment.WebRootPath, "images", "photos", imageFileName);

                    using (var stream = new FileStream(imagePathWithFileName, FileMode.Create))
                    {
                        await profileImage.CopyToAsync(stream);
                    }

                    // Update the user's ProfileImage property with the new image file name
                    user.ProfileImage = imageFileName;

                    // Update the user in the database
                    var result = await _userHelper.UpdateUserAsync(user);

                    if (result.Succeeded)
                    {
                        TempData["UserUpdatedMessage"] = "Profile image updated successfully.";
                        return RedirectToAction("UserList"); // Redirect to UserList or ChangeUser as needed
                    }
                    else
                    {
                        // Handle errors, e.g., result.Errors
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "User not found.");
                }
            }

            // Redirect to UserList or ChangeUser with errors as needed
            return RedirectToAction("UserList"); // Redirect to UserList or ChangeUser as needed
        }

        public IActionResult RecoverPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RecoverPassword(RecoverPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userHelper.GetUserByEmailAsync(model.Email);
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "The email doesn't correspond to a registered user.");
                    return View(model);
                }

                var myToken = await _userHelper.GeneratePasswordResetTokenAsync(user);

                var link = this.Url.Action(
                    "ResetPassword",
                    "Account",
                    new { token = myToken }, protocol: HttpContext.Request.Scheme);

                Response response = await _mailHelper.SendEmail(model.Email, "Account Password Reset", $"<h1>Account Password Reset</h1>" +
                    $"To reset the password, click the link:</br></br>" +
                    $"<a href = \"{link}\">Reset Password</a>");

                if (response.IsSuccess)
                {
                    TempData["SuccessMessage"] = "The instructions to recover your password have been sent to your email.";
                    return RedirectToAction("RecoverPasswordConfirmation"); // Redirect to a confirmation view
                }

                TempData["ErrorMessage"] = "Error while resetting the password!";
                return RedirectToAction("RecoverPasswordConfirmation"); // Redirect to a confirmation view
            }

            TempData["ErrorMessage"] = "User not found!";
            return View(model);
        }

        public IActionResult RecoverPasswordConfirmation()
        {
            return View();
        }


        public IActionResult ResetPassword(string token)
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            var user = await _userHelper.GetUserByEmailAsync(model.Username);
            if (user != null)
            {
                var result = await _userHelper.ResetPasswordAsync(user, model.Token, model.Password);
                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "Password reset successful.";

                    // Remove the error message if it exists
                    TempData.Remove("ErrorMessage");

                    // Return the view instead of JSON
                    return View(model);
                }

                TempData["ErrorMessage"] = "Error while resetting the password.";
                return View(model);
            }

            TempData["ErrorMessage"] = "User not found.";
            return View(model);
        }

        public IActionResult RegistrationRequestConfirmation()
        {
            return View();
        }


        [AllowAnonymous]
        public IActionResult RegistrationRequest()
        {
            var model = new AccountRegistrationRequestViewModel();
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrationRequest(AccountRegistrationRequestViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Create a registration request entity and save it to the database
                var registrationRequest = new RegistrationRequest
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Username = model.Username,
                    Email = model.Email,
                    // Set other properties as needed
                };

                // Save the registration request to your DataContext
                _context.RegistrationRequests.Add(registrationRequest);
                await _context.SaveChangesAsync();

                // Notify the admin about the new registration request (send an email)
                await SendRegistrationRequestEmail(registrationRequest);

                // Redirect the user to a confirmation page or show a thank you message
                return RedirectToAction("RegistrationRequestConfirmation");
            }

            return View(model);
        }

        private async Task SendRegistrationRequestEmail(RegistrationRequest request)
        {
            var adminEmail = "tarikeduardo3@gmail.com"; // Replace with your admin's email address
            var subject = "New Registration Request";
            var message = $"You have received a new registration request from {request.FirstName} {request.LastName}. Email: {request.Email}.";

            // Use your mail helper to send the email to the admin
            await _mailHelper.SendEmail(adminEmail, subject, message);
        }


    }
}
