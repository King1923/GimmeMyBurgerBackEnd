using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace LearningAPI.Models
{
	public class Order
	{
		// Primary Key
		[Key]
		public int OrderId { get; set; }

		// SKU: Stock Keeping Unit
		[Required]
		public int SKU { get; set; }

		[Required, MinLength(3), MaxLength(100)]
		public string Name { get; set; } = string.Empty;

		[Required, Column(TypeName = "decimal(10,2)")]
		public decimal Price { get; set; }

		[Required]
		public string Description { get; set; } = string.Empty;

		[Required, MaxLength(100)]
		public string Category { get; set; } = string.Empty;

		// Image field
		[MaxLength(20)]
		public string? ImageFile { get; set; }

		[Required]
		public int Stock { get; set; }

		[Column(TypeName = "datetime")]
		public DateTime CreatedAt { get; set; }

		[Column(TypeName = "datetime")]
		public DateTime UpdatedAt { get; set; }
	}
}