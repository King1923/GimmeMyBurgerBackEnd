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

		// Foreign key for User
		[Required]
		public int UserId { get; set; }

		// Foreign key for Product
		[Required]
		public int ProductId { get; set; }

		[Required]
		public int Quantity { get; set; }

		[Required, Column(TypeName = "decimal(10,2)")]
		public decimal TotalAmount { get; set; }

		// Navigation properties
		[JsonIgnore]
		[ForeignKey("UserId")]
		public User? User { get; set; }

		[JsonIgnore]
		[ForeignKey("ProductId")]
		public Product? Product { get; set; }
	}
}