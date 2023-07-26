using LigaNOS.Data.Entities;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace LigaNOS.Helpers
{
    public interface IUserHelper
    {
        Task<CustomUser> GetUserByEmailAsync(string email);

        Task<IdentityResult> AddUserAsync(CustomUser user, string password);
    }
}
