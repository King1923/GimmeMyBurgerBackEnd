using LearningAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LearningAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TutorialController(MyDbContext context) : ControllerBase
    {
        private readonly MyDbContext _context = context;

        [HttpGet]
        public IActionResult GetAll(string? search)
        {
            IQueryable<Tutorial> result = _context.Tutorials.Include(t => t.User);
            if (search != null)
            {
                result = result.Where(x => x.Title.Contains(search)
                    || x.Description.Contains(search));
            }
            var list = result.OrderByDescending(x => x.CreatedAt).ToList();
            var data = list.Select(t => new
            {
                t.Id,
                t.Title,
                t.Description,
                t.ImageFile,
                t.CreatedAt,
                t.UpdatedAt,
                t.UserId,
                User = new
                {
                    t.User?.Name
                }
            });
            return Ok(data);
        }

        [HttpGet("{id}")]
        public IActionResult GetTutorial(int id)
        {
            Tutorial? tutorial = _context.Tutorials.Include(t => t.User)
                .SingleOrDefault(t => t.Id == id);
            if (tutorial == null)
            {
                return NotFound();
            }
            var data = new
            {
                tutorial.Id,
                tutorial.Title,
                tutorial.Description,
                tutorial.ImageFile,
                tutorial.CreatedAt,
                tutorial.UpdatedAt,
                tutorial.UserId,
                User = new
                {
                    tutorial.User?.Name
                }
            };
            return Ok(data);
        }

        [HttpPost, Authorize]
        public IActionResult AddTutorial(Tutorial tutorial)
        {
            int userId = GetUserId();
            var now = DateTime.Now;
            var myTutorial = new Tutorial()
            {
                Title = tutorial.Title.Trim(),
                Description = tutorial.Description.Trim(),
                ImageFile = tutorial.ImageFile,
                CreatedAt = now,
                UpdatedAt = now,
                UserId = userId
            };

            _context.Tutorials.Add(myTutorial);
            _context.SaveChanges();
            return Ok(myTutorial);
        }

        [HttpPut("{id}"), Authorize]
        public IActionResult UpdateTutorial(int id, Tutorial tutorial)
        {
            var myTutorial = _context.Tutorials.Find(id);
            if (myTutorial == null)
            {
                return NotFound();
            }

            int userId = GetUserId();
            if (myTutorial.UserId != userId)
            {
                return Forbid();
            }

            myTutorial.Title = tutorial.Title.Trim();
            myTutorial.Description = tutorial.Description.Trim();
            myTutorial.ImageFile = tutorial.ImageFile;
            myTutorial.UpdatedAt = DateTime.Now;

            _context.SaveChanges();
            return Ok();
        }

        [HttpDelete("{id}"), Authorize]
        public IActionResult DeleteTutorial(int id)
        {
            var myTutorial = _context.Tutorials.Find(id);
            if (myTutorial == null)
            {
                return NotFound();
            }

            int userId = GetUserId();
            if (myTutorial.UserId != userId)
            {
                return Forbid();
            }

            _context.Tutorials.Remove(myTutorial);
            _context.SaveChanges();
            return Ok();
        }

        private int GetUserId()
        {
            return Convert.ToInt32(User.Claims
                .Where(c => c.Type == ClaimTypes.NameIdentifier)
                .Select(c => c.Value).SingleOrDefault());
        }
    }
}
