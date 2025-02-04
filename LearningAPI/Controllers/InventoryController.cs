using LearningAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace LearningAPI.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class InventoryController(MyDbContext context) : ControllerBase
	{
		private readonly MyDbContext _context = context;

		[HttpGet]
		public IActionResult GetAll(string? search)
		{
			IQueryable<Inventory> result = _context.Inventories;

			if (!string.IsNullOrEmpty(search))
			{
				// Attempt to parse the search string as an integer
				if (int.TryParse(search, out int quantity))
				{
					// Search by Item or Quantity
					result = result.Where(x => x.Item.Contains(search) || x.Quantity == quantity);
				}
				else
				{
					// If not a valid number, only search by Item
					result = result.Where(x => x.Item.Contains(search));
				}
			}

			var list = result.OrderByDescending(x => x.CreatedAt).ToList();
			return Ok(list);
		}

		[HttpGet("{id}")]
		public IActionResult GetInventory(int id)
		{
			Inventory? inventory = _context.Inventories.Find(id);
			if (inventory == null)
			{
				return NotFound();
			}
			return Ok(inventory);
		}


		[HttpPost]
		public IActionResult AddInventory(Inventory inventory)
		{
			var now = DateTime.Now;
			var myInventory = new Inventory()
			{
				Item = inventory.Item.Trim(),
				Quantity = inventory.Quantity,
				CreatedAt = now,
				UpdatedAt = now
			};
			_context.Inventories.Add(myInventory);
			_context.SaveChanges();
			return Ok(myInventory);
		}

		[HttpPut("{id}")]
		public IActionResult UpdateInventory(int id, Inventory inventory)
		{
			var myInventory = _context.Inventories.Find(id);
			if (myInventory == null)
			{
				return NotFound();
			}

			myInventory.Item = inventory.Item.Trim();
			myInventory.Quantity = inventory.Quantity;
			myInventory.UpdatedAt = DateTime.Now;

			_context.SaveChanges();
			return Ok();
		}

		[HttpDelete("{id}")]
		public IActionResult DeleteInventory(int id)
		{
			var myInventory = _context.Inventories.Find(id);
			if (myInventory == null)
			{
				return NotFound();
			}

			_context.Inventories.Remove(myInventory);
			_context.SaveChanges();
			return Ok();
		}
	}
}
