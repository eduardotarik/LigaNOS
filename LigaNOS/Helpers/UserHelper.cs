using LigaNOS.Data.Entities;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace LigaNOS.Helpers
{
    public class UserHelper : IUserHelper
    {
        private readonly UserManager<CustomUser> _userManager;

        public UserHelper(UserManager<CustomUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IdentityResult> AddUserAsync(CustomUser user, string password)
        {
            return await _userManager.CreateAsync(user, password);
        }

        public async Task<CustomUser> GetUserByEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }
    }
}
