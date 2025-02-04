using System.ComponentModel.DataAnnotations;

namespace LearningAPI.Models
{
    public class AddRewardRequest
    {
        [Required, MinLength(3), MaxLength(100)] 
        public string Title { get; set; } = string.Empty;

        [Required, MinLength(3), MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? ImageFile { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Points required must be a non-negative value.")]
        public int PointsRequired { get; set; }

        [Required]
        [DataType(DataType.Date, ErrorMessage = "Invalid expiry date format")]
        public DateTime ExpiryDate { get; set; }

        [Required, MaxLength(100)]
        public string RewardType { get; set; } = string.Empty;
    }
}