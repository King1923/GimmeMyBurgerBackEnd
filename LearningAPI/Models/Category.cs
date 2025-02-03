using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LearningAPI.Models
{
	public class Category
	{
		// Primary Key
		[Key]
		public int CategoryId { get; set; }

		// Category name should only contain alphabet letters
		[Required, MinLength(3), MaxLength(100)]
		[RegularExpression("^[A-Za-z]+$", ErrorMessage = "Category name can only contain alphabet letters.")]
		public string CategoryName { get; set; } = string.Empty;

		[Column(TypeName = "datetime")]
		public DateTime CreatedAt { get; set; }

		[Column(TypeName = "datetime")]
		public DateTime UpdatedAt { get; set; }
	}
}