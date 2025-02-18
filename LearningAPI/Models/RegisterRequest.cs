using System.ComponentModel.DataAnnotations;

namespace LearningAPI.Models
{
    public class RegisterRequest
    {
        [Required, MinLength(3), MaxLength(50)]
        [RegularExpression(@"^[a-zA-Z '-,.]+$",
            ErrorMessage = "Only allow letters, spaces and characters: ' - , .")]
        public string FName { get; set; } = string.Empty;

        [Required, MinLength(3), MaxLength(50)]
        [RegularExpression(@"^[a-zA-Z '-,.]+$",
            ErrorMessage = "Only allow letters, spaces and characters: ' - , .")]
        public string LName { get; set; } = string.Empty;

        [Required, EmailAddress, MaxLength(50)]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(8), MaxLength(50)]
        [RegularExpression(@"^(?=.*[a-zA-Z])(?=.*[0-9]).{8,}$",
            ErrorMessage = "At least 1 letter and 1 number")]
        public string Password { get; set; } = string.Empty;

        [Required, MaxLength(8)]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "Mobile number must contain only digits")]
        public string Mobile { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        public DateTime DoB { get; set; }

        public string Role { get; set; } = "User"; // Default to "User"
    }
}
