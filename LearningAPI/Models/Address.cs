using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace LearningAPI.Models
{
    public class Address
    {
        public int Id { get; set; }

        // Foreign key
        public int UserId { get; set; }

        [MaxLength(20)]
        public string BlockNumber { get; set; } = string.Empty;

        [MaxLength(100)]
        public string StreetName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string BuildingName { get; set; } = string.Empty;

        [MaxLength(20)]
        public string UnitNumber { get; set; } = string.Empty;

        [RegularExpression(@"^\d{6}$", ErrorMessage = "Postal code must be exactly 6 digits")]
        public string PostalCode { get; set; } = string.Empty;

        // Optionally, include geolocation fields:
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        [JsonIgnore]
        public User? User { get; set; }
    }
}
