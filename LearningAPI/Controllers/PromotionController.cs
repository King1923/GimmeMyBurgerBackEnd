using LearningAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace LearningAPI.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class PromotionController(MyDbContext context) : ControllerBase
	{
		private readonly MyDbContext _context = context;

		[HttpGet]
		public IActionResult GetAll(string? search)
		{
			IQueryable<Promotion> result = _context.Promotions;
			if (search != null)
			{
				result = result.Where(x => x.Title.Contains(search) || x.Description.Contains(search));
			}
			var list = result.OrderByDescending(x => x.CreatedAt).ToList();
			return Ok(list);
		}

		[HttpGet("{id}")]
		public IActionResult GetPromotion(int id)
		{
			Promotion? promotion = _context.Promotions.Find(id);
			if (promotion == null)
			{
				return NotFound();
			}
			return Ok(promotion);
		}

		[HttpPost]
		public IActionResult AddPromotion(Promotion promotion)
		{
			var now = DateTime.Now;
			var myPromotion = new Promotion()
			{
				Title = promotion.Title.Trim(),
				Description = promotion.Description.Trim(),
				ImageFile = promotion.ImageFile,
				CreatedAt = now,
				UpdatedAt = now
			};
			_context.Promotions.Add(myPromotion);
			_context.SaveChanges();
			return Ok(myPromotion);
		}

		[HttpPut("{id}")]
		public IActionResult UpdatePromotion(int id, Promotion promotion)
		{
			var myPromotion = _context.Promotions.Find(id);
			if (myPromotion == null)
			{
				return NotFound();
			}
			myPromotion.Title = promotion.Title.Trim();
			myPromotion.Description = promotion.Description.Trim();
			myPromotion.UpdatedAt = DateTime.Now;
			myPromotion.ImageFile = promotion.ImageFile;

			_context.SaveChanges();
			return Ok();
		}

		[HttpDelete("{id}")]
		public IActionResult DeletePromotion(int id)
		{
			var myPromotion = _context.Promotions.Find(id);
			if (myPromotion == null)
			{
				return NotFound();
			}
			_context.Promotions.Remove(myPromotion);
			_context.SaveChanges();
			return Ok();
		}
	}
}
