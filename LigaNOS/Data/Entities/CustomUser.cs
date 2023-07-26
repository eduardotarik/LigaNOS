using Microsoft.AspNetCore.Identity;

namespace LigaNOS.Data.Entities
{
    public class CustomUser : IdentityUser
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }
    }
}
