using System.ComponentModel.DataAnnotations;

namespace RecyclingApp.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public string Phone { get; set; }

        public string Address { get; set; }

        [Required]
        public string Role { get; set; }
    }
}
