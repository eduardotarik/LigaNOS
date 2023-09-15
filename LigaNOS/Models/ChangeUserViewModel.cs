using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace LigaNOS.Models
{
    public class ChangeUserViewModel
    {
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Display(Name = "Profile Image")]
        public IFormFile ProfileImage { get; set; }

        public string CurrentProfileImage { get; set; }

        public string UserUpdatedMessage { get; set; }
    }
}
