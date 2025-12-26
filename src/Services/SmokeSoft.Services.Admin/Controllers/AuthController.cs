using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SmokeSoft.Services.Admin.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SmokeSoft.Services.Admin.Controllers;

[ApiController]
[Route("api/admin/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AdminDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthController(AdminDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] AdminLoginRequest request)
    {
        // Find admin user by email
        var admin = await _context.AdminUsers
            .FirstOrDefaultAsync(u => u.Email == request.Email && u.IsActive);

        if (admin == null)
        {
            return Unauthorized(new { message = "Invalid email or password" });
        }

        // Verify password (BCrypt)
        if (!BCrypt.Net.BCrypt.Verify(request.Password, admin.PasswordHash))
        {
            return Unauthorized(new { message = "Invalid email or password" });
        }

        // Generate JWT token
        var token = GenerateJwtToken(admin);

        // Update last login
        admin.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new
        {
            accessToken = token,
            user = new
            {
                id = admin.Id,
                email = admin.Email,
                username = admin.Username,
                firstName = admin.FirstName,
                lastName = admin.LastName,
                role = admin.Role
            }
        });
    }

    private string GenerateJwtToken(Data.Entities.AdminUser admin)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"];
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, admin.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, admin.Email),
            new Claim(ClaimTypes.Name, admin.Username),
            new Claim(ClaimTypes.Role, admin.Role),
            new Claim("firstName", admin.FirstName),
            new Claim("lastName", admin.LastName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(int.Parse(jwtSettings["ExpirationMinutes"]!)),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public record AdminLoginRequest(string Email, string Password);
