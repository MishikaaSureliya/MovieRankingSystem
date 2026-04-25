using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MovieRankingSystem.Data;
using MovieRankingSystem.Models;
using MovieRankingSystem.Views.Home;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Serilog;

namespace MovieRankingSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public AuthController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpPost("register")]
        public IActionResult Register(RegisterModel model)
        {
            try
            { 
            Log.Information("Register attempt for Email: {Email}", model.Email);

                if (!ModelState.IsValid)
                {
                    Log.Warning("Validation failed for {Email}", model.Email);
                    return BadRequest(ModelState);
                }

                if (_context.Users.Any(u => u.Email == model.Email || u.Name == model.Name))
                {
                    Log.Warning("User already exists: {Email}", model.Email);
                    return BadRequest(new { message = "User already exists" });
                }
                var user = new User
                {
                   Name = model.Name,
                   Email = model.Email,
                   PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),

                        // ✅ Audit fields
                   CreatedBy = model.Name,
                   CreatedDate = DateTime.Now,
                   IsDeleted = false
                };

                _context.Users.Add(user);
                _context.SaveChanges();
                Log.Information("User registered successfully: {Email}", model.Email);
                return Ok("Registered successfully");

            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error during registration for {Email}", model?.Email);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginModel model)
        {
            try
            { 
                Log.Information("Login attempt: {Email}", model.Email);
                if (model == null)
                return BadRequest("Invalid data");

                var user = _context.Users.FirstOrDefault(u => u.Email == model.Email);

                if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
                {
                    Log.Warning("Invalid login attempt: {Email}", model.Email);
                    return Unauthorized("Invalid credentials");
                }
                Log.Warning("Invalid login attempt: {Email}", model.Email);
                var token = GenerateToken(user);
                Log.Information("Login successful: {Email}", model.Email);
                return Ok(new { token });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error during login for {Email}", model?.Email);
                return StatusCode(500, "Internal server error");
            }
        }

        private string GenerateToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
            new Claim(ClaimTypes.Name, user.Email)
        };

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [HttpPut("update")]
        public IActionResult UpdateUser(int id, string newName)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id && !u.IsDeleted);

            if (user == null)
                return NotFound();

            user.Name = newName;

            // ✅ Audit fields
            user.UpdatedBy = User.Identity?.Name ?? "system";
            user.UpdatedDate = DateTime.Now;

            _context.SaveChanges();

            return Ok("User updated");
        }
        [HttpDelete("delete")]
        public IActionResult DeleteUser(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id && !u.IsDeleted);

            if (user == null)
                return NotFound();

            // ❌ DO NOT REMOVE FROM DB
            // _context.Users.Remove(user);

            // ✅ Soft delete
            user.IsDeleted = true;
            user.UpdatedBy = User.Identity?.Name ?? "system";
            user.UpdatedDate = DateTime.Now;

            _context.SaveChanges();

            return Ok("User deleted (soft)");
        }
    }
}

