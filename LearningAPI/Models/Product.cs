using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LearningAPI.Models
{
	public class Product
	{
		[Key]
		public int ProductId { get; set; }

		// SKU should only allow integers
		[Required]
		[Range(1, int.MaxValue, ErrorMessage = "SKU must be a positive integer.")]
		public int SKU { get; set; }

		// Name should allow alphabet letters and spaces
		[Required, MinLength(3), MaxLength(100)]
		[RegularExpression("^[A-Za-z ]+$", ErrorMessage = "Name can only contain alphabet letters and spaces.")]
		public string Name { get; set; } = string.Empty;

		// Price should only allow decimals
		[Required, Column(TypeName = "decimal(10,2)")]
		[Range(0.01, double.MaxValue, ErrorMessage = "Price must be a positive decimal value.")]
		public decimal Price { get; set; }

		// Description should have a maximum of 1000 characters and allow spaces
		[Required]
		[MaxLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
		public string Description { get; set; } = string.Empty;

		// ImageFile can be nullable but have a max length of 20 characters
		[MaxLength(20)]
		public string? ImageFile { get; set; }

		// Stock should only allow integers
		[Required]
		[Range(0, int.MaxValue, ErrorMessage = "Stock must be a non-negative integer.")]
		public int Stock { get; set; }

		[Column(TypeName = "datetime")]
		public DateTime CreatedAt { get; set; }

		[Column(TypeName = "datetime")]
		public DateTime UpdatedAt { get; set; }

		// Foreign Key
		[Required]
		public int CategoryId { get; set; }

		public Category? Category { get; set; }
	}
}