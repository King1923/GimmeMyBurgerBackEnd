using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Text.Json.Serialization;

namespace LearningAPI.Models
{
	public class Cart
	{
		[Key]
		public int CartId { get; set; }

		// Foreign key for Product
		[Required]
		public int ProductId { get; set; }

		// Foreign key for User
		[Required]
		public int UserId { get; set; }

		[Required]
		public int Quantity { get; set; }

		// Computed field (not stored in DB)
		[NotMapped]
		public decimal TotalAmount => (Product != null ? Product.Price : 0) * Quantity;

		// Navigation properties
		[JsonIgnore]
		[ForeignKey("UserId")]
		public User? User { get; set; }

		[ForeignKey("ProductId")]
		public Product? Product { get; set; }
	}
}