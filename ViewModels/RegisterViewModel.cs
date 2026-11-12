using System.ComponentModel.DataAnnotations;

namespace RecyclingApp.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Full name is required.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 50 characters.")]
        [RegularExpression(@"^[a-zA-Z\u0600-\u06FF\s]+$", ErrorMessage = "Name can contain letters and spaces only.")]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression(@"^01[0125][0-9]{8}$", ErrorMessage = "Phone must be 11 digits and start with 010, 011, 012 or 015.")]
        public string Phone { get; set; }

        public string Address { get; set; }

        [Required]
        public string Role { get; set; }
    }
}