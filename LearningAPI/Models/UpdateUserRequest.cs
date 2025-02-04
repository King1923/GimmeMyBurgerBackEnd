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

        [MaxLength(150)]
        public string? DeliveryAddress { get; set; }

        [MinLength(8), MaxLength(50)]
        public string? Password { get; set; }

        [Range(100000, 999999)]  // Assuming postal codes are 6-digit numbers
        public int? PostalCode { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DoB { get; set; }
    }
}
