using LearningAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LearningAPI.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class CartController : ControllerBase
	{
		private readonly MyDbContext _context;

		public CartController(MyDbContext context)
		{
			_context = context;
		}

        // Get all cart items for a specific user
        [HttpGet, Authorize]
        public IActionResult GetUserCart()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var cartItems = _context.Carts
                .Where(c => c.UserId == userId)
                .Include(c => c.Product)
                .Select(c => new
                {
                    c.CartId,
                    c.ProductId,
                    ProductName = c.Product.Name,
                    c.Quantity,
                    c.TotalAmount,
                    Price = c.Product.Price,         // Include product price
                    Category = c.Product.Category,   // Include product category
                    ImageFile = c.Product.ImageFile  // Include product image
                })
                .ToList();

            return Ok(cartItems);
        }

        // Add an item to the cart
        [HttpPost, Authorize]
        public IActionResult AddToCart([FromBody] Cart cart)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            // Ensure the product exists
            var product = _context.Products.Find(cart.ProductId);
            if (product == null)
            {
                return NotFound("Product not found.");
            }

            var existingCartItem = _context.Carts
                .FirstOrDefault(c => c.UserId == userId && c.ProductId == cart.ProductId);

            if (existingCartItem != null)
            {
                existingCartItem.Quantity += cart.Quantity;
            }
            else
            {
                var newCartItem = new Cart
                {
                    UserId = userId,
                    ProductId = cart.ProductId,
                    Quantity = cart.Quantity
                };

                _context.Carts.Add(newCartItem);
            }

            _context.SaveChanges();
            return Ok(new { message = "Item added to cart successfully!" });
        }

        // Update the quantity of an item in the cart
        [HttpPut("{cartId}"), Authorize]
		public IActionResult UpdateCart(int cartId, [FromBody] Cart cart)
		{
			var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

			var existingCartItem = _context.Carts
				.FirstOrDefault(c => c.CartId == cartId && c.UserId == userId);

			if (existingCartItem == null)
			{
				return NotFound("Cart item not found.");
			}

			existingCartItem.Quantity = cart.Quantity;
			_context.SaveChanges();

			return Ok("Cart updated.");
		}

		// Remove an item from the cart
		[HttpDelete("{cartId}"), Authorize]
		public IActionResult DeleteCartItem(int cartId)
		{
			var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

			var cartItem = _context.Carts
				.FirstOrDefault(c => c.CartId == cartId && c.UserId == userId);

			if (cartItem == null)
			{
				return NotFound("Cart item not found.");
			}

			_context.Carts.Remove(cartItem);
			_context.SaveChanges();

			return Ok("Item removed from cart.");
		}
	}
}