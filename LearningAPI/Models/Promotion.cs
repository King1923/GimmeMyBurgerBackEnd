using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LearningAPI.Models
{
	public class Promotion
	{
		public int Id { get; set; }

		[Required, MinLength(3), MaxLength(100)]
		public string Title { get; set; } = string.Empty;

		[Required, MinLength(3), MaxLength(500)]
		public string Description { get; set; } = string.Empty;

		[MaxLength(20)]
		public string? ImageFile { get; set; }

		[Column(TypeName = "datetime")]
		public DateTime CreatedAt { get; set; }

		[Column(TypeName = "datetime")]
		public DateTime UpdatedAt { get; set; }
	}
}
