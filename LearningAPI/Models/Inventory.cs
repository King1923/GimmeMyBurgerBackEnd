using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LearningAPI.Models
{
	public class Inventory
	{
		public int Id { get; set; }

		[Required, MinLength(3), MaxLength(100)]
		public string Item { get; set; } = string.Empty;
		public int Quantity { get; set; }

		[Column(TypeName = "datetime")]
		public DateTime CreatedAt { get; set; }

		[Column(TypeName = "datetime")]
		public DateTime UpdatedAt { get; set; }
	}
}
