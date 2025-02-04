using LearningAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace LearningAPI.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class ProductController : ControllerBase
	{
		private readonly MyDbContext _context;

		public ProductController(MyDbContext context)
		{
			_context = context;
		}

		[HttpGet]
		public IActionResult GetAll(string? search)
		{
			IQueryable<Product> result = _context.Products;

			if (search != null)
			{
				result = result.Where(p => p.Name.Contains(search)
					|| p.Description.Contains(search));
			}

			var list = result.OrderByDescending(x => x.CreatedAt).ToList();
			var data = list.Select(p => new
			{
				p.ProductId,
				p.SKU,
				p.Name,
				p.Description,
				p.Price,
				p.Stock,
				p.ImageFile,
				p.CreatedAt,
				p.UpdatedAt,
				p.CategoryId
			});

			return Ok(data);
		}

		[HttpGet("{id}")]
		public IActionResult GetProduct(int id)
		{
			Product? product = _context.Products
				.SingleOrDefault(t => t.ProductId == id);

			if (product == null)
			{
				return NotFound();
			}

			var data = new
			{
				product.ProductId,
				product.SKU,
				product.Name,
				product.Description,
				product.Price,
				product.Stock,
				product.ImageFile,
				product.CreatedAt,
				product.UpdatedAt,
				product.CategoryId
			};

			return Ok(data);
		}

		[HttpPost, Authorize]
		public async Task<IActionResult> AddProduct(Product product)
		{
			// Validate CategoryId
			if (!await _context.Categories.AnyAsync(c => c.CategoryId == product.CategoryId))
			{
				return BadRequest("Invalid Category ID.");
			}

			// Validate SKU (positive integer)
			if (product.SKU <= 0)
			{
				return BadRequest("SKU must be a positive integer.");
			}

			// Validate Name (letters and spaces only)
			if (!Regex.IsMatch(product.Name, "^[A-Za-z ]+$"))
			{
				return BadRequest("Name can only contain alphabet letters and spaces.");
			}

			// Validate Price (decimal value greater than 0)
			if (product.Price <= 0)
			{
				return BadRequest("Price must be a positive decimal value.");
			}

			// Validate Stock (non-negative integer)
			if (product.Stock < 0)
			{
				return BadRequest("Stock must be a non-negative integer.");
			}

			// Validate Description (max length of 1000)
			if (product.Description.Length > 1000)
			{
				return BadRequest("Description cannot exceed 1000 characters.");
			}

			// Assign CreatedAt
			product.CreatedAt = DateTime.Now;

			_context.Products.Add(product);
			await _context.SaveChangesAsync();
			return Ok(product);
		}

		[HttpPut("{id}"), Authorize]
		public async Task<IActionResult> UpdateProduct(int id, [FromBody] Product updatedProduct)
		{
			// Validate if the ID matches the ProductId in the payload
			if (id != updatedProduct.ProductId)
				return BadRequest("Product ID mismatch.");

			// Validate if the CategoryId exists
			if (!await _context.Categories.AnyAsync(c => c.CategoryId == updatedProduct.CategoryId))
				return BadRequest("Invalid Category ID.");

			// Validate SKU (positive integer)
			if (updatedProduct.SKU <= 0)
			{
				return BadRequest("SKU must be a positive integer.");
			}

			// Validate Name (letters and spaces only)
			if (!Regex.IsMatch(updatedProduct.Name, "^[A-Za-z ]+$"))
			{
				return BadRequest("Name can only contain alphabet letters and spaces.");
			}

			// Validate Price (decimal value greater than 0)
			if (updatedProduct.Price <= 0)
			{
				return BadRequest("Price must be a positive decimal value.");
			}

			// Validate Stock (non-negative integer)
			if (updatedProduct.Stock < 0)
			{
				return BadRequest("Stock must be a non-negative integer.");
			}

			// Validate Description (max length of 1000)
			if (updatedProduct.Description.Length > 1000)
			{
				return BadRequest("Description cannot exceed 1000 characters.");
			}

			// Retrieve the existing product
			var product = await _context.Products.FindAsync(id);
			if (product == null)
				return NotFound();

			// Update the product details
			product.Name = updatedProduct.Name;
			product.Description = updatedProduct.Description;
			product.Price = updatedProduct.Price;
			product.Stock = updatedProduct.Stock;
			product.SKU = updatedProduct.SKU;
			product.ImageFile = updatedProduct.ImageFile;
			product.CategoryId = updatedProduct.CategoryId;
			product.UpdatedAt = DateTime.Now;

			// Save changes to the database
			await _context.SaveChangesAsync();
			return Ok(product);
		}

		[HttpDelete("{id}"), Authorize]
		public IActionResult DeleteProduct(int id)
		{
			var myProduct = _context.Products.Find(id);
			if (myProduct == null)
			{
				return NotFound();
			}

			_context.Products.Remove(myProduct);
			_context.SaveChanges();
			return Ok();
		}
	}
}