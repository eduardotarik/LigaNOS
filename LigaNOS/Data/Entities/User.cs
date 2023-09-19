using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace LigaNOS.Data.Entities
{
    public class User : IdentityUser
    {

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string ProfileImage { get; set; }

        public ICollection<Game> Games { get; set; }

        // Navigation property for associated teams
        public ICollection<Team> Teams { get; set; }

        public ICollection<Player> Players { get; set; }
    }
}
