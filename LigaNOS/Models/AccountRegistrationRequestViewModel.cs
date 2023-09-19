using System.ComponentModel.DataAnnotations;

namespace LigaNOS.Models
{
    public class AccountRegistrationRequestViewModel
    {
        [Required]
        [MaxLength(255)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(255)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(255)]
        [Display(Name = "Email")]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        [MaxLength(255)]
        [Display(Name = "Email Confirmation")]
        [Compare("Username", ErrorMessage = "The emails must match.")]
        public string Email { get; set; }

        public string Password { get; set; }
    }
}
