using System.Collections.Generic;
using User = LigaNOS.Data.Entities.User;

namespace LigaNOS.Models
{
    public class UserWithRolesViewModel
    {
        public List<string> Roles { get; set; }

        public User User { get; set; }

        public string UserListUrl { get; set; }
    }
}
