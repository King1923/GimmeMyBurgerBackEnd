using System;
using System.ComponentModel.DataAnnotations;

namespace LearningAPI.Models
{
    public class UpdateAddressRequest
    {
        [Required]
        public int Id { get; set; }

        [MinLength(1), MaxLength(20)]
        public string? BlockNumber { get; set; }

        [MinLength(1), MaxLength(100)]
        public string? StreetName { get; set; }

        [MinLength(1), MaxLength(100)]
        public string? BuildingName { get; set; }

        [MinLength(1), MaxLength(20)]
        public string? UnitNumber { get; set; }

        [RegularExpression(@"^\d{6}$", ErrorMessage = "Postal code must be exactly 6 digits")]
        public string? PostalCode { get; set; }

        // Optional geolocation fields
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}
