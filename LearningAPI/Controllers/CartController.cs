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


		[HttpGet("{id}")]
		public IActionResult GetCart(int id)
		{
			Cart? cart = _context.Carts
				.SingleOrDefault(t => t.CartId == id);

			if (cart == null)
			{
				return NotFound();
			}

			var data = new
			{
				cart.CartId,
				cart.Quantity,
				cart.TotalAmount
			};

			return Ok(data);
		}

		[HttpPost, Authorize]
		public IActionResult AddCart(Cart cart)
		{
			var now = DateTime.Now;
			var myCart = new Cart()
			{
				TotalAmount = cart.TotalAmount,
				Quantity = cart.Quantity
			};

			_context.Carts.Add(myCart);
			_context.SaveChanges();

			return Ok(myCart);
		}

		[HttpPut("{id}"), Authorize]
		public IActionResult UpdateCart(int id, Cart cart)
		{
			var myCart = _context.Carts.Find(id);
			if (myCart == null)
			{
				return NotFound();
			}

			myCart.TotalAmount= cart.TotalAmount;
			myCart.Quantity= cart.Quantity;

			_context.SaveChanges();
			return Ok();
		}

		[HttpDelete("{id}"), Authorize]
		public IActionResult DeleteCart(int id)
		{
			var myCart = _context.Carts.Find(id);
			if (myCart == null)
			{
				return NotFound();
			}

			_context.Carts.Remove(myCart);
			_context.SaveChanges();
			return Ok();
		}
	}
}