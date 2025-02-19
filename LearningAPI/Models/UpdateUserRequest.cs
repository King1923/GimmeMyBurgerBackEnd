using System.ComponentModel.DataAnnotations;

namespace LearningAPI.Models
{
    public class UpdateUserRequest
    {
        [MinLength(3), MaxLength(100)]
        public string? FName { get; set; }

        [MinLength(3), MaxLength(100)]
        public string? LName { get; set; }

        [EmailAddress, MaxLength(50)]
        public string? Email { get; set; }

        [MaxLength(15)]
        public string? Mobile { get; set; }

        [MinLength(8), MaxLength(50)]
        public string? Password { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DoB { get; set; }
    }
}
