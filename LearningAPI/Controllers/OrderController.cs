using LearningAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;


namespace LearningAPI.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class OrderController : ControllerBase
	{
		private readonly MyDbContext _context;

		public OrderController(MyDbContext context)
		{
			_context = context;
		}

		[HttpGet]
		public IActionResult GetAll(string? search)
		{
			IQueryable<Order> result = _context.Orders;

			if (search != null)
			{
				result = result.Where(o => o.Name.Contains(search)
					|| o.Description.Contains(search)
					|| o.Category.Contains(search));
			}

			var list = result.OrderByDescending(x => x.CreatedAt).ToList();
			var data = list.Select(o => new
			{
				o.OrderId,
				o.SKU,
				o.Name,
				o.Description,
				o.Price,
				o.Category,
				o.Stock,
				o.ImageFile,
				o.CreatedAt,
				o.UpdatedAt
			});

			return Ok(data);
		}

		[HttpGet("{id}")]
		public IActionResult GetOrder(int id)
		{
			Order? order = _context.Orders
				.SingleOrDefault(t => t.OrderId == id);

			if (order == null)
			{
				return NotFound();
			}

			var data = new
			{
				order.OrderId,
				order.SKU,
				order.Name,
				order.Description,
				order.Price,
				order.Category,
				order.Stock,
				order.ImageFile,
				order.CreatedAt,
				order.UpdatedAt
			};

			return Ok(data);
		}


		[HttpPut("{id}"), Authorize]
		public IActionResult UpdateOrder(int id, Order order)
		{
			var myOrder = _context.Orders.Find(id);
			if (myOrder == null)
			{
				return NotFound();
			}

			myOrder.SKU = order.SKU;
			myOrder.Name = order.Name.Trim();
			myOrder.Description = order.Description.Trim();
			myOrder.Price = order.Price;
			myOrder.Category = order.Category.Trim();
			myOrder.Stock = order.Stock;
			myOrder.ImageFile = order.ImageFile;
			myOrder.UpdatedAt = DateTime.Now;

			_context.SaveChanges();
			return Ok();
		}

		[HttpDelete("{id}"), Authorize]
		public IActionResult DeleteOrder(int id)
		{
			var myOrder = _context.Orders.Find(id);
			if (myOrder == null)
			{
				return NotFound();
			}

			_context.Orders.Remove(myOrder);
			_context.SaveChanges();
			return Ok();
		}
	}
}