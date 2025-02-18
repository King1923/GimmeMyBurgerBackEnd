using LearningAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LearningAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AddressController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly ILogger<AddressController> _logger;

        public AddressController(MyDbContext context, ILogger<AddressController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: /Address?search=...
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Address>), StatusCodes.Status200OK)]
        public IActionResult GetAll(string? search)
        {
            try
            {
                IQueryable<Address> result = _context.Addresses;
                if (!string.IsNullOrWhiteSpace(search))
                {
                    result = result.Where(a =>
                        a.BlockNumber.Contains(search) ||
                        a.StreetName.Contains(search) ||
                        a.BuildingName.Contains(search) ||
                        a.UnitNumber.Contains(search) ||
                        a.PostalCode.Contains(search)
                    );
                }
                // Order the addresses by Id in descending order.
                // If you have a CreatedAt field, you might use that instead.
                var list = result.OrderByDescending(a => a.Id).ToList();
                return Ok(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when getting all addresses");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }


        // GET: /Address/{id}
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Address), StatusCodes.Status200OK)]
        public IActionResult GetAddress(int id)
        {
            try
            {
                // Include the related User data similar to the Reward GET endpoint.
                Address? address = _context.Addresses
                    .Include(a => a.User)
                    .SingleOrDefault(a => a.Id == id);
                if (address == null)
                {
                    return NotFound(new { message = "Address not found" });
                }
                return Ok(address);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching address {AddressId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }


        // POST: /Address
        [HttpPost, Authorize]
        [ProducesResponseType(typeof(Address), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateAddress([FromBody] AddAddressRequest addressRequest)
        {
            try
            {
                int userId = GetUserId();
                // Create a new address object using the Address class directly
                var newAddress = new Address()
                {
                    BlockNumber = addressRequest.BlockNumber.Trim(),
                    StreetName = addressRequest.StreetName.Trim(),
                    BuildingName = addressRequest.BuildingName.Trim(),
                    UnitNumber = addressRequest.UnitNumber.Trim(),
                    PostalCode = addressRequest.PostalCode.Trim(),
                    Latitude = addressRequest.Latitude,
                    Longitude = addressRequest.Longitude,
                    UserId = userId
                };

                _context.Addresses.Add(newAddress);
                await _context.SaveChangesAsync();

                // Optionally, fetch the newly created address including related User data
                var createdAddress = await _context.Addresses
                    .Include(a => a.User)
                    .FirstOrDefaultAsync(a => a.Id == newAddress.Id);

                return Ok(createdAddress);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating address");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // PUT: /Address/{id}
        [HttpPut("{id}"), Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateAddress(int id, [FromBody] UpdateAddressRequest addressRequest)
        {
            try
            {
                if (id != addressRequest.Id)
                {
                    return BadRequest(new { message = "Address ID mismatch" });
                }

                var address = await _context.Addresses.FindAsync(id);
                if (address == null)
                {
                    return NotFound(new { message = "Address not found" });
                }

                int userId = GetUserId();
                if (address.UserId != userId)
                {
                    return Forbid();
                }

                // Update address properties if provided (trim string fields)
                if (!string.IsNullOrWhiteSpace(addressRequest.BlockNumber))
                {
                    address.BlockNumber = addressRequest.BlockNumber.Trim();
                }
                if (!string.IsNullOrWhiteSpace(addressRequest.StreetName))
                {
                    address.StreetName = addressRequest.StreetName.Trim();
                }
                if (!string.IsNullOrWhiteSpace(addressRequest.BuildingName))
                {
                    address.BuildingName = addressRequest.BuildingName.Trim();
                }
                if (!string.IsNullOrWhiteSpace(addressRequest.UnitNumber))
                {
                    address.UnitNumber = addressRequest.UnitNumber.Trim();
                }
                if (!string.IsNullOrWhiteSpace(addressRequest.PostalCode))
                {
                    address.PostalCode = addressRequest.PostalCode.Trim();
                }
                if (addressRequest.Latitude.HasValue)
                {
                    address.Latitude = addressRequest.Latitude.Value;
                }
                if (addressRequest.Longitude.HasValue)
                {
                    address.Longitude = addressRequest.Longitude.Value;
                }

                await _context.SaveChangesAsync();

                return Ok(new { message = "Address updated successfully", address });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating address {AddressId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // DELETE: /Address/{id}
        [HttpDelete("{id}"), Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteAddress(int id)
        {
            try
            {
                var address = await _context.Addresses.FindAsync(id);
                if (address == null)
                {
                    return NotFound(new { message = "Address not found" });
                }

                int userId = GetUserId();
                if (address.UserId != userId)
                {
                    return Forbid();
                }

                _context.Addresses.Remove(address);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Address deleted successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting address {AddressId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // Helper method to get the user ID from the JWT token
        private int GetUserId()
        {
            return Convert.ToInt32(User.Claims
                .Where(c => c.Type == ClaimTypes.NameIdentifier)
                .Select(c => c.Value)
                .SingleOrDefault());
        }
    }
}
 