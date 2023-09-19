using System.ComponentModel.DataAnnotations;
using System;

namespace LigaNOS.Models
{
    public class RegistrationRequest
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(255)]
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

        [Required]
        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
