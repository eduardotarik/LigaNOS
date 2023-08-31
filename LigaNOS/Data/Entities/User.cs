using Microsoft.AspNetCore.Identity;

namespace LigaNOS.Data.Entities
{
    public class User : IdentityUser
    {

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string ProfileImage { get; set; }
    }
}
