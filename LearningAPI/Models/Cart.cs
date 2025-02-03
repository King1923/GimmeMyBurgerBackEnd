using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace LearningAPI.Models
{
	public class Cart
	{
		[Key]
		public int CartId { get; set; }

		[Required]
		public int Quantity { get; set; }

		[Required, Column(TypeName = "decimal(10,2)")]
		public decimal TotalAmount { get; set; }
	}
}