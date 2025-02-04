using LearningAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;

namespace LearningAPI.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class CategoryController : ControllerBase
	{
		private readonly MyDbContext _context;

		public CategoryController(MyDbContext context)
		{
			_context = context;
		}

		[HttpGet]
		public IActionResult GetAll(string? search)
		{
			IQueryable<Category> result = _context.Categories;

			if (search != null)
			{
				result = result.Where(ca => ca.CategoryName.Contains(search));
			}

			var list = result.OrderByDescending(x => x.CreatedAt).ToList();
			var data = list.Select(ca => new
			{
				ca.CategoryId,
				ca.CategoryName,
				ca.CreatedAt,
				ca.UpdatedAt
			});

			return Ok(data);
		}

		[HttpGet("{id}")]
		public IActionResult GetCategory(int id)
		{
			Category? category = _context.Categories
				.SingleOrDefault(t => t.CategoryId == id);

			if (category == null)
			{
				return NotFound();
			}

			var data = new
			{
				category.CategoryId,
				category.CategoryName,
				category.CreatedAt,
				category.UpdatedAt
			};

			return Ok(data);
		}

		[HttpPost, Authorize]
		public IActionResult AddCategory(Category category)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var now = DateTime.Now;
			var myCategory = new Category()
			{
				CategoryName = category.CategoryName.Trim(),
				CreatedAt = now,
				UpdatedAt = now
			};

			_context.Categories.Add(myCategory);
			_context.SaveChanges();

			return Ok(myCategory);
		}

		[HttpPut("{id}"), Authorize]
		public IActionResult UpdateCategory(int id, Category category)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var myCategory = _context.Categories.Find(id);
			if (myCategory == null)
			{
				return NotFound();
			}

			myCategory.CategoryName = category.CategoryName.Trim();
			myCategory.UpdatedAt = DateTime.Now;

			_context.SaveChanges();
			return Ok();
		}

		[HttpDelete("{id}"), Authorize]
		public IActionResult DeleteCategory(int id)
		{
			var myCategory = _context.Categories.Find(id);
			if (myCategory == null)
			{
				return NotFound();
			}

			_context.Categories.Remove(myCategory);
			_context.SaveChanges();
			return Ok();
		}
	}
}