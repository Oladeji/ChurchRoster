using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ChurchRoster.Application.DTOs.Auth;
using ChurchRoster.Application.Interfaces;
using ChurchRoster.Core.Entities;
using ChurchRoster.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ChurchRoster.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<AuthResponse?> LoginAsync(LoginRequest request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null || !user.IsActive)
                return null;

            if (!VerifyPassword(request.Password, user.PasswordHash))
                return null;

            var token = GenerateJwtToken(user.UserId, user.Email, user.Role);

            var expirationMinutes = int.Parse(_configuration["JwtSettings:ExpirationInMinutes"] ?? "1440");
            var expiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes);

            return new AuthResponse(
                user.UserId,
                user.Name,
                user.Email,
                user.Role,
                token,
                expiresAt
            );
        }

        public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
        {
            // Check if email already exists
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                return null;

            // Validate password requirements (min 8 chars, uppercase, lowercase, number)
            if (!IsValidPassword(request.Password))
                return null;

            var user = new User
            {
                Name = request.Name,
                Email = request.Email,
                Phone = request.Phone,
                PasswordHash = HashPassword(request.Password),
                Role = "Member", // Default role for new registrations
                IsActive = true,
                MonthlyLimit = 4, // Default monthly limit
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var token = GenerateJwtToken(user.UserId, user.Email, user.Role);

            var expirationMinutes = int.Parse(_configuration["JwtSettings:ExpirationInMinutes"] ?? "1440");
            var expiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes);

            return new AuthResponse(
                user.UserId,
                user.Name,
                user.Email,
                user.Role,
                token,
                expiresAt
            );
        }

        public string GenerateJwtToken(int userId, string email, string role)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];
            var expirationMinutes = int.Parse(jwtSettings["ExpirationInMinutes"] ?? "1440");

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim(ClaimTypes.Role, role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool VerifyPassword(string password, string passwordHash)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, passwordHash);
            }
            catch
            {
                return false;
            }
        }

        private bool IsValidPassword(string password)
        {
            // Minimum 8 characters, at least one uppercase, one lowercase, and one number
            if (password.Length < 8)
                return false;

            bool hasUpperCase = password.Any(char.IsUpper);
            bool hasLowerCase = password.Any(char.IsLower);
            bool hasDigit = password.Any(char.IsDigit);

            return hasUpperCase && hasLowerCase && hasDigit;
        }
    }
}
