using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace LearningAPI.Models
{

    public enum RoleType
    {
        User,
        Staff
    }
    public class User
    {
        public int Id { get; set; }

        // Foreign key property
        public int RoleId { get; set; }

        [MaxLength(100)]
        public string FName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string LName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(15)]
        public string Mobile { get; set; } = string.Empty;

        [MaxLength(100), JsonIgnore]
        public string Password { get; set; } = string.Empty;

        [MaxLength(150)]
        public string DeliveryAddress { get; set; } = string.Empty;

        [Column(TypeName = "datetime")]
        public DateTime DoB { get; set; }

        public int PostalCode {  get; set; }

        public int PaymentMethodId { get; set; }

        public int PointsEarned { get; set; }

        // Role Type (Customer, Staff)
        public RoleType Role { get; set; } = RoleType.User; // Default to "Customer"

        [Column(TypeName = "datetime")]
        public DateTime CreatedAt { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime UpdatedAt { get; set; }

        // Navigation property to represent the one-to-many relationship
        [JsonIgnore]
        public List<Reward>? Rewards { get; set; }

    }
}