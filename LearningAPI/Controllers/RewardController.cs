using LearningAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LearningAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RewardController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly ILogger<RewardController> _logger;

        public RewardController(MyDbContext context, ILogger<RewardController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET /reward?search=...
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Reward>), StatusCodes.Status200OK)]
        public IActionResult GetAll(string? search)
        {
            try
            {
                IQueryable<Reward> result = _context.Rewards.Include(r => r.User);
                if (!string.IsNullOrWhiteSpace(search))
                {
                    result = result.Where(r => r.Title.Contains(search) ||
                                               r.Description.Contains(search));
                }
                var list = result.OrderByDescending(r => r.CreatedAt).ToList();
                return Ok(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when getting all rewards");
                return StatusCode(500);
            }
        }

        // GET /reward/{id}
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Reward), StatusCodes.Status200OK)]
        public IActionResult GetReward(int id)
        {
            try
            {
                Reward? reward = _context.Rewards.Include(r => r.User)
                    .SingleOrDefault(r => r.Id == id);
                if (reward == null)
                {
                    return NotFound();
                }
                return Ok(reward);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when getting reward by id");
                return StatusCode(500);
            }
        }

        // POST /reward
        [HttpPost, Authorize]
        [ProducesResponseType(typeof(Reward), StatusCodes.Status200OK)]
        public IActionResult AddReward(AddRewardRequest rewardRequest)
        {
            try
            {
                int userId = GetUserId();
                var now = DateTime.Now;

                // Create a new reward object using the Reward class directly
                var newReward = new Reward()
                {
                    Title = rewardRequest.Title.Trim(),
                    Description = rewardRequest.Description.Trim(),
                    ImageFile = rewardRequest.ImageFile,
                    PointsRequired = rewardRequest.PointsRequired,
                    ExpiryDate = rewardRequest.ExpiryDate,
                    RewardType = rewardRequest.RewardType.Trim(),
                    CreatedAt = now,
                    UpdatedAt = now,
                    UserId = userId
                };

                // Add the reward to the database and save changes
                _context.Rewards.Add(newReward);
                _context.SaveChanges();

                // Optionally, fetch the newly created reward including related User data
                var createdReward = _context.Rewards.Include(r => r.User)
                    .FirstOrDefault(r => r.Id == newReward.Id);

                return Ok(createdReward);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when adding reward");
                return StatusCode(500);
            }
        }

        // PUT /reward/{id}
        [HttpPut("{id}"), Authorize]
        public IActionResult UpdateReward(int id, UpdateRewardRequest rewardRequest)
        {
            try
            {
                // Find the existing reward by id
                var reward = _context.Rewards.Find(id);
                if (reward == null)
                {
                    return NotFound();
                }

                // Ensure the logged-in user is the owner of the reward
                int userId = GetUserId();
                if (reward.UserId != userId)
                {
                    return Forbid();
                }

                // Update reward fields if provided
                if (!string.IsNullOrWhiteSpace(rewardRequest.Title))
                {
                    reward.Title = rewardRequest.Title.Trim();
                }

                if (!string.IsNullOrWhiteSpace(rewardRequest.Description))
                {
                    reward.Description = rewardRequest.Description.Trim();
                }

                if (rewardRequest.ImageFile != null)
                {
                    reward.ImageFile = rewardRequest.ImageFile;
                }

                if (rewardRequest.PointsRequired.HasValue)
                {
                    reward.PointsRequired = rewardRequest.PointsRequired.Value;
                }

                if (rewardRequest.ExpiryDate.HasValue)
                {
                    reward.ExpiryDate = rewardRequest.ExpiryDate.Value;
                }

                if (!string.IsNullOrWhiteSpace(rewardRequest.RewardType))
                {
                    reward.RewardType = rewardRequest.RewardType.Trim();
                }

                // Update the timestamp
                reward.UpdatedAt = DateTime.Now;

                // Save changes
                _context.SaveChanges();
                return Ok(new { message = "Reward updated successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when updating reward");
                return StatusCode(500);
            }
        }

        // DELETE /reward/{id}
        [HttpDelete("{id}"), Authorize]
        public IActionResult DeleteReward(int id)
        {
            try
            {
                var reward = _context.Rewards.Find(id);
                if (reward == null)
                {
                    return NotFound();
                }

                int userId = GetUserId();
                if (reward.UserId != userId)
                {
                    return Forbid();
                }

                _context.Rewards.Remove(reward);
                _context.SaveChanges();
                return Ok(new { message = "Reward deleted successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when deleting reward");
                return StatusCode(500);
            }
        }

        private int GetUserId()
        {
            return Convert.ToInt32(User.Claims
                .Where(c => c.Type == ClaimTypes.NameIdentifier)
                .Select(c => c.Value)
                .SingleOrDefault());
        }
    }
}
