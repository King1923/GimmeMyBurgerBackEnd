using System.ComponentModel.DataAnnotations;

namespace LearningAPI.Models
{
    public class UpdateRewardRequest
    {
        [MinLength(3), MaxLength(100)]
        public string? Title { get; set; }

        [MinLength(3), MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(20)]
        public string? ImageFile { get; set; }

        public int? PointsRequired { get; set; }  // New field: PointsRequired (nullable)

        public DateTime? ExpiryDate { get; set; } // New field: ExpiryDate (nullable)

        [MaxLength(100)]
        public string? RewardType { get; set; }   // New field: RewardType (nullable)
    }
}
