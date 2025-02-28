﻿using LearningAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LearningAPI.Helpers;
using System.Security.Cryptography;

namespace LearningAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserController> _logger;
        private readonly IMemoryCache _cache;
        private readonly EmailService _emailService;

        public UserController(MyDbContext context, IConfiguration configuration, ILogger<UserController> logger, IMemoryCache cache, EmailService emailService)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
            _cache = cache;
            _emailService = emailService;
        }

        [HttpGet]
        public IActionResult GetAll()   
        {
            IQueryable<User> result = _context.Users;
            var list = result.OrderByDescending(x => x.CreatedAt).ToList();
            return Ok(list);
        }

        [HttpPost("register")]
        public IActionResult Register(RegisterRequest request)
        {
            try
            {
                // Trim string values
                request.FName = request.FName.Trim();
                request.LName = request.LName.Trim();
                request.Email = request.Email.Trim().ToLower();
                request.Password = request.Password.Trim();
                request.Mobile = request.Mobile.Trim();

                // Validate Role (it should be either "User" or "Staff")
                var allowedRoles = new List<string> { "User", "Staff" };
                if (!allowedRoles.Contains(request.Role))
                {
                    string message = "Invalid role type. Allowed values: User, Staff.";
                    return BadRequest(new { message });
                }

                // Check if the email already exists
                var foundUser = _context.Users.FirstOrDefault(x => x.Email == request.Email);
                if (foundUser != null)
                {
                    string message = "Email already exists.";
                    return BadRequest(new { message });
                }

                // Convert the Role string to RoleType enum
                RoleType roleType = Enum.Parse<RoleType>(request.Role, true);

                // Create user object
                var now = DateTime.Now;
                string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
                var user = new User
                {
                    FName = request.FName,
                    LName = request.LName,
                    Email = request.Email,
                    Password = passwordHash,
                    Mobile = request.Mobile,
                    DoB = request.DoB,
                    Role = roleType,  // Assign the RoleType enum (User or Staff)
                    CreatedAt = now,
                    UpdatedAt = now
                };

                // Add the new user to the database and save changes
                _context.Users.Add(user);
                _context.SaveChanges();
                return Ok(new { message = "Registration successful." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when user registers");
                return StatusCode(500);
            }
        }
        [HttpPost("login")]
        public IActionResult Login(LoginRequest request)
        {
            try
            {
                // Trim and normalize input.
                request.Email = request.Email.Trim().ToLower();
                request.Password = request.Password.Trim();

                // Look up the user by email.
                var foundUser = _context.Users.FirstOrDefault(x => x.Email == request.Email);
                if (foundUser == null)
                {
                    return BadRequest(new { message = "Email or password is not correct." });
                }

                // Verify password.
                bool verified = BCrypt.Net.BCrypt.Verify(request.Password, foundUser.Password);
                if (!verified)
                {
                    return BadRequest(new { message = "Email or password is not correct." });
                }

                // Generate a secure 6-digit 2FA code.
                var twoFaCode = GenerateSecureResetCode();

                // Generate a temporary token
                string tempToken = Guid.NewGuid().ToString();

                // Store the code in cache using the normalized email as key, with a 3-minute expiration.
                _cache.Set($"2FA_{foundUser.Email}", twoFaCode, TimeSpan.FromMinutes(3));

                // Send the 2FA code via email.
                _emailService.Send2faEmail(foundUser.Email, twoFaCode);

                // Return the temporary token along with a message.
                return Ok(new { message = "A 2FA code has been sent to your email. Please verify to complete login." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return StatusCode(500);
            }
        }


        /*
                [HttpPost("login")]
                public IActionResult Login(LoginRequest request)
                {
                    try
                    {
                        // Trim string values
                        request.Email = request.Email.Trim().ToLower();
                        request.Password = request.Password.Trim();

                        // Look up the user by email
                        var foundUser = _context.Users.FirstOrDefault(x => x.Email == request.Email);
                        if (foundUser == null)
                        {
                            return BadRequest(new { message = "Email or password is not correct." });
                        }

                        // Verify the password using BCrypt
                        bool verified = BCrypt.Net.BCrypt.Verify(request.Password, foundUser.Password);
                        if (!verified)
                        {
                            return BadRequest(new { message = "Email or password is not correct." });
                        }

                        // Create a JWT token for the authenticated user
                        string accessToken = CreateToken(foundUser);

                        // Ensure sensitive data (like the hashed password) is not returned
                        foundUser.Password = null;

                        // Return the user (using the User class directly) along with the token
                        var response = new LoginResponse
                        {
                            User = foundUser,
                            AccessToken = accessToken
                        };
                        return Ok(response);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error when user login");
                        return StatusCode(500);
                    }
                } */

        [HttpGet("auth"), Authorize]
        public IActionResult Auth()
        {
            try
            {
                // Retrieve the user ID from the JWT token claims
                var idClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(idClaim, out int userId))
                {
                    return Unauthorized();
                }

                // Retrieve the full user object from the database
                var user = _context.Users.Find(userId);
                if (user == null)
                {
                    return Unauthorized();
                }

                // Clear sensitive data before sending the user data
                user.Password = null;
                return Ok(new AuthResponse { User = user });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when user auth");
                return StatusCode(500);
            }
        }

        [HttpPut("{id}"), Authorize]
        public IActionResult UpdateUser(int id, UpdateUserRequest request)
        {
            try
            {
                // Find the user by ID
                var myUser = _context.Users.Find(id);
                if (myUser == null)
                {
                    return NotFound(new { message = "User not found." });
                }

                // Ensure the logged-in user is authorized to update this account
                int userId = GetUserId();
                if (myUser.Id != userId)
                {
                    return Forbid();
                }

                // Validate and update the email if it has changed
                if (!string.IsNullOrWhiteSpace(request.Email) &&
                    request.Email.Trim().ToLower() != myUser.Email)
                {
                    var existingUser = _context.Users.FirstOrDefault(x => x.Email == request.Email.Trim().ToLower());
                    if (existingUser != null)
                    {
                        return BadRequest(new { message = "Email already exists." });
                    }
                    myUser.Email = request.Email.Trim().ToLower();
                }

                // Update other fields if provided
                if (!string.IsNullOrWhiteSpace(request.FName))
                {
                    myUser.FName = request.FName.Trim();
                }
                if (!string.IsNullOrWhiteSpace(request.LName))
                {
                    myUser.LName = request.LName.Trim();
                }
                if (!string.IsNullOrWhiteSpace(request.Mobile))
                {
                    myUser.Mobile = request.Mobile.Trim();
                }
                if (!string.IsNullOrWhiteSpace(request.Password))
                {
                    // Hash the new password before saving
                    myUser.Password = BCrypt.Net.BCrypt.HashPassword(request.Password.Trim());
                }
                if (request.DoB.HasValue)
                {
                    myUser.DoB = request.DoB.Value;
                }

                myUser.UpdatedAt = DateTime.Now;
                _context.SaveChanges();

                return Ok(new { message = "User details updated successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when updating user details");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }


        [HttpPut("admin/{id}"), Authorize(Roles = "Staff")]
        public IActionResult AdminUpdateUser(int id, UpdateUserRequest request)
        {
            try
            {
                // Find the user by ID
                var userToUpdate = _context.Users.Find(id);
                if (userToUpdate == null)
                {
                    return NotFound(new { message = "User not found." });
                }

                // Update email if provided and changed
                if (!string.IsNullOrWhiteSpace(request.Email) &&
                    request.Email.Trim().ToLower() != userToUpdate.Email)
                {
                    var existingUser = _context.Users.FirstOrDefault(x => x.Email == request.Email.Trim().ToLower());
                    if (existingUser != null)
                    {
                        return BadRequest(new { message = "Email already exists." });
                    }
                    userToUpdate.Email = request.Email.Trim().ToLower();
                }

                // Update other fields if provided
                if (!string.IsNullOrWhiteSpace(request.FName))
                {
                    userToUpdate.FName = request.FName.Trim();
                }
                if (!string.IsNullOrWhiteSpace(request.LName))
                {
                    userToUpdate.LName = request.LName.Trim();
                }
                if (!string.IsNullOrWhiteSpace(request.Mobile))
                {
                    userToUpdate.Mobile = request.Mobile.Trim();
                }
                if (!string.IsNullOrWhiteSpace(request.Password))
                {
                    // Hash the new password before saving
                    userToUpdate.Password = BCrypt.Net.BCrypt.HashPassword(request.Password.Trim());
                }
                if (request.DoB.HasValue)
                {
                    userToUpdate.DoB = request.DoB.Value;
                }

                userToUpdate.UpdatedAt = DateTime.Now;
                _context.SaveChanges();

                return Ok(new { message = "User details updated successfully by admin." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when updating user details by admin");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }




        [HttpDelete("{id}"), Authorize]
        public IActionResult DeleteUser(int id)
        {
            try
            {
                // Find the user by ID
                var myUser = _context.Users.Find(id);
                if (myUser == null)
                {
                    return NotFound(new { message = "User not found." });
                }

                // Ensure the logged-in user is allowed to delete this account
                int userId = GetUserId();
                if (myUser.Id != userId)
                {
                    return Forbid();
                }

                // Remove the user from the database
                _context.Users.Remove(myUser);
                _context.SaveChanges();

                return Ok(new { message = "User deleted successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when deleting user");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }


        [HttpDelete("admin/{id}"), Authorize(Roles = "Staff")]
        public IActionResult AdminDeleteUser(int id)
        {
            try
            {
                // Find the user by ID
                var myUser = _context.Users.Find(id);
                if (myUser == null)
                {
                    return NotFound(new { message = "User not found." });
                }

                // Remove the user from the database
                _context.Users.Remove(myUser);
                _context.SaveChanges();

                return Ok(new { message = "User deleted successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when deleting user");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUserById(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }
            user.Password = null; // Remove sensitive info before sending
            return Ok(user);
        }

        private int GetUserId()
        {
            // Get the User ID from the JWT token claim (NameIdentifier)
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                throw new UnauthorizedAccessException("User is not authenticated.");
            }
            return int.Parse(userIdClaim);
        }

        private string CreateToken(User user)
        {
            string? secret = _configuration.GetValue<string>("Authentication:Secret");
            if (string.IsNullOrEmpty(secret))
            {
                throw new Exception("Secret is required for JWT authentication.");
            }

            int tokenExpiresDays = _configuration.GetValue<int>("Authentication:TokenExpiresDays");
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secret);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())

            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(tokenExpiresDays),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var securityToken = tokenHandler.CreateToken(tokenDescriptor);
            string token = tokenHandler.WriteToken(securityToken);
            return token;
        }
        // Request model for requesting a reset code.
        public class ResetRequest
        {
            public string Email { get; set; }
        }

        // Request model for verifying the reset code.
        public class ResetCodeVerificationRequest
        {
            public string Email { get; set; }
            public string Code { get; set; }
        }

        // Request model for resetting the password.
        public class ResetPasswordRequest
        {
            public string Email { get; set; }
            public string NewPassword { get; set; }
        }

        // Helper method to generate a secure 6-digit reset code.
        private string GenerateSecureResetCode()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                var bytes = new byte[4];
                rng.GetBytes(bytes);
                int code = BitConverter.ToInt32(bytes, 0) % 900000;
                code = Math.Abs(code) + 100000; // Ensures a 6-digit number.
                return code.ToString("D6");
            }
        }

        // POST: /User/request-password-reset
        [HttpPost("request-password-reset")]
        public IActionResult RequestPasswordReset([FromBody] ResetRequest request)
        {
            try
            {
                // Validate that an email is provided.
                if (string.IsNullOrWhiteSpace(request.Email))
                {
                    return BadRequest(new { message = "Email is required." });
                }

                // Normalize the email.
                string email = request.Email.Trim().ToLower();

                // Retrieve the user by email.
                var user = _context.Users.FirstOrDefault(x => x.Email == email);
                if (user == null)
                {
                    return NotFound(new { message = "User not found." });
                }

                // Generate a secure 6-digit reset code.
                var resetCode = GenerateSecureResetCode();

                // Store the reset code securely with an expiration (10 minutes)
                // using the normalized email as the cache key.
                _cache.Set($"Reset_{email}", resetCode, TimeSpan.FromMinutes(10));

                // Send email with the reset code using the EmailService.
                _emailService.SendResetEmail(user.Email, resetCode);

                return Ok(new { message = "Password reset code sent successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending password reset code.");
                return StatusCode(500, new { message = "Error sending password reset code." });
            }
        }


        // POST: /User/verify-reset-code
        [HttpPost("verify-reset-code")]
        public IActionResult VerifyResetCode([FromBody] ResetCodeVerificationRequest request)
        {
            try
            {
                // Validate that an email is provided.
                if (string.IsNullOrWhiteSpace(request.Email))
                {
                    return BadRequest(new { message = "Email is required." });
                }

                // Normalize the email.
                string email = request.Email.Trim().ToLower();

                // Retrieve the stored reset code using the key "Reset_{email}".
                if (!_cache.TryGetValue($"Reset_{email}", out string storedCode))
                {
                    return BadRequest(new { message = "Reset code expired or not found." });
                }

                // Compare the provided code with the stored code.
                if (request.Code == storedCode)
                {
                    // Clear the stored code now that it's been used.
                    _cache.Remove($"Reset_{email}");
                    return Ok(new { message = "Reset code verified successfully." });
                }
                else
                {
                    return BadRequest(new { message = "Invalid reset code." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying reset code.");
                return StatusCode(500, new { message = "Error verifying reset code." });
            }
        }




        // POST: /User/reset-password
        [HttpPost("reset-password")]
        public IActionResult ResetPassword([FromBody] ResetPasswordRequest request)
        {
            try
            {
                // Validate that an email is provided.
                if (string.IsNullOrWhiteSpace(request.Email))
                {
                    return BadRequest(new { message = "Email is required." });
                }

                // Normalize the email.
                string email = request.Email.Trim().ToLower();

                // Retrieve the user by email.
                var user = _context.Users.FirstOrDefault(x => x.Email == email);
                if (user == null)
                {
                    return NotFound(new { message = "Email not found." });
                }

                // Hash the new password.
                user.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
                user.UpdatedAt = DateTime.Now;
                _context.SaveChanges();

                return Ok(new { message = "Password reset successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password.");
                return StatusCode(500, new { message = "Error resetting password." });
            }
        }

        [HttpPost("change-password"), Authorize]
        public IActionResult ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                int userId = GetUserId();
                var user = _context.Users.Find(userId);
                if (user == null)
                {
                    return NotFound(new { message = "User not found." });
                }
                // Verify the current password.
                bool valid = BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.Password);
                if (!valid)
                {
                    return BadRequest(new { message = "Current password is incorrect." });
                }
                // Update password
                user.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
                user.UpdatedAt = DateTime.Now;
                _context.SaveChanges();
                return Ok(new { message = "Password changed successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password.");
                return StatusCode(500, new { message = "Error changing password." });
            }
        }

        public class ChangePasswordRequest
        {
            public string CurrentPassword { get; set; }
            public string NewPassword { get; set; }
            public string ConfirmPassword { get; set; }
        }

        // Request models for 2FA verification
        public class Verify2FARequest
        {
            public string Email { get; set; }
            public string Code { get; set; }
        }

        [HttpPost("verify-2fa")]
        public IActionResult Verify2FA([FromBody] Verify2FARequest request)
        {
            try
            {
                // Validate email input.
                if (string.IsNullOrWhiteSpace(request.Email))
                {
                    return BadRequest(new { message = "Email is required." });
                }

                string email = request.Email.Trim().ToLower();

                // Try to retrieve the stored 2FA code from cache.
                if (!_cache.TryGetValue($"2FA_{email}", out string storedCode))
                {
                    return BadRequest(new { message = "2FA code expired or not found." });
                }

                // Compare the provided code.
                if (request.Code != storedCode)
                {
                    return BadRequest(new { message = "Invalid 2FA code." });
                }

                // Remove the code from the cache as it's been successfully used.
                _cache.Remove($"2FA_{email}");

                // Retrieve the user (for token generation).
                var user = _context.Users.FirstOrDefault(x => x.Email == email);
                if (user == null)
                {
                    return NotFound(new { message = "User not found." });
                }

                // Generate JWT token.
                string accessToken = CreateToken(user);

                // Clear sensitive data.
                user.Password = null;

                // Return final token and user info.
                var response = new LoginResponse
                {
                    User = user,
                    AccessToken = accessToken
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying 2FA code");
                return StatusCode(500);
            }
        }

        [HttpPost("resend-2fa")]
        public IActionResult Resend2FA([FromBody] Resend2FARequest request)
        {
            try
            {
                // Validate that an email is provided.
                if (string.IsNullOrWhiteSpace(request.Email))
                {
                    return BadRequest(new { message = "Email is required." });
                }

                // Normalize the email.
                string email = request.Email.Trim().ToLower();

                // Retrieve the user by email.
                var user = _context.Users.FirstOrDefault(x => x.Email == email);
                if (user == null)
                {
                    return NotFound(new { message = "User not found." });
                }

                // Generate a new secure 6-digit 2FA code.
                var new2FACode = GenerateSecureResetCode();

                // Store the new 2FA code in the cache with a 3-minute expiration.
                _cache.Set($"2FA_{email}", new2FACode, TimeSpan.FromMinutes(3));

                // Send the new 2FA code via email.
                _emailService.Send2faEmail(email, new2FACode);

                return Ok(new { message = "A new 2FA code has been sent to your email." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resending 2FA code");
                return StatusCode(500, new { message = "Error resending 2FA code." });
            }
        }

        // Request model for resending 2FA code.
        public class Resend2FARequest
        {
            public string Email { get; set; }
        }

    }
}
