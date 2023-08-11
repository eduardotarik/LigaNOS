using LigaNOS.Data.Entities;
using LigaNOS.Models;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LigaNOS.Helpers
{
    public interface IUserHelper
    {
        Task<User> GetUserByEmailAsync(string email);

        Task<IdentityResult> AddUserAsync(User user, string password);

        Task<SignInResult> LoginAsync(LoginViewModel model);

        Task LogoutAsync();

        Task CheckRoleAsync(string roleName);

        Task AddUserToRoleAsync(User user, string roleName);

        Task<bool> IsUserInRoleAsync(User user, string roleName);

        Task<IdentityResult> CreateUserAsync(User user, string password, string roleName);

        Task<IdentityResult> DeleteUserAsync(User user);

        Task<User> GetUserByIdAsync(string userId);

        Task<List<User>> GetAllUsersAsync();
    }
}
