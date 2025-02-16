using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LearningAPI.Models;  // Adjust the namespace as needed

namespace LearningAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class MarkersController : ControllerBase
	{
		private readonly MyDbContext _context;

		public MarkersController(MyDbContext context)
		{
			_context = context;
		}

		// GET: api/Markers
		[HttpGet]
		public async Task<ActionResult<IEnumerable<Marker>>> GetMarkers()
		{
			return await _context.Markers.ToListAsync();
		}

		// GET: api/Markers/5
		[HttpGet("{id}")]
		public async Task<ActionResult<Marker>> GetMarker(int id)
		{
			var marker = await _context.Markers.FindAsync(id);

			if (marker == null)
			{
				return NotFound();
			}

			return marker;
		}

		// POST: api/Markers
		[HttpPost]
		public async Task<ActionResult<Marker>> PostMarker(Marker marker)
		{
			_context.Markers.Add(marker);
			await _context.SaveChangesAsync();

			// Returns a 201 status code with the newly created marker
			return CreatedAtAction(nameof(GetMarker), new { id = marker.Id }, marker);
		}

		// PUT: api/Markers/5
		[HttpPut("{id}")]
		public async Task<IActionResult> PutMarker(int id, Marker marker)
		{
			if (id != marker.Id)
			{
				return BadRequest();
			}

			_context.Entry(marker).State = EntityState.Modified;

			try
			{
				await _context.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!MarkerExists(id))
				{
					return NotFound();
				}
				else
				{
					throw;
				}
			}

			// Returns a 204 No Content response
			return NoContent();
		}

		// DELETE: api/Markers/5
		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteMarker(int id)
		{
			var marker = await _context.Markers.FindAsync(id);
			if (marker == null)
			{
				return NotFound();
			}

			_context.Markers.Remove(marker);
			await _context.SaveChangesAsync();

			// Returns a 204 No Content response
			return NoContent();
		}

		private bool MarkerExists(int id)
		{
			return _context.Markers.Any(e => e.Id == id);
		}
	}
}
