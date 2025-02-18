using System.ComponentModel.DataAnnotations;

namespace LearningAPI.Models
{
    public class AddAddressRequest
    {
        [Required, MaxLength(20)]
        public string BlockNumber { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string StreetName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string BuildingName { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string UnitNumber { get; set; } = string.Empty;

        [Required, RegularExpression(@"^\d{6}$", ErrorMessage = "Postal code must be exactly 6 digits")]
        public string PostalCode { get; set; } = string.Empty;

        // Optional: Include geolocation fields if needed
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}
