using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ChurchRoster.Application.DTOs.Auth;
using ChurchRoster.Application.Interfaces;
using ChurchRoster.Core.Entities;
using ChurchRoster.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace ChurchRoster.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(AppDbContext context, IConfiguration configuration, ILogger<AuthService> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<AuthResponse?> LoginAsync(LoginRequest request)
        {
            try
            {
                _logger.LogInformation("Login attempt for email: {Email}", request.Email);

                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == request.Email);

                if (user == null)
                {
                    _logger.LogWarning("Login failed: User not found for email: {Email}", request.Email);
                    return null;
                }

                if (!user.IsActive)
                {
                    _logger.LogWarning("Login failed: User is not active for email: {Email}", request.Email);
                    return null;
                }

                _logger.LogDebug("User found: {UserId}, verifying password...", user.UserId);

                if (!VerifyPassword(request.Password, user.PasswordHash))
                {
                    _logger.LogWarning("Login failed: Invalid password for email: {Email}", request.Email);
                    return null;
                }

                _logger.LogDebug("Password verified, generating JWT token...");

                var token = GenerateJwtToken(user.UserId, user.Email, user.Role);

                var expirationMinutes = int.Parse(_configuration["JwtSettings:ExpirationInMinutes"] ?? "1440");
                var expiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes);

                _logger.LogInformation("Login successful for user: {UserId}, email: {Email}", user.UserId, request.Email);

                return new AuthResponse(
                    user.UserId,
                    user.Name,
                    user.Email,
                    user.Role,
                    token,
                    expiresAt
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for email: {Email}", request.Email);
                throw;
            }
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
            try
            {
                _logger.LogDebug("Generating JWT token for user: {UserId}, email: {Email}, role: {Role}", userId, email, role);

                var jwtSettings = _configuration.GetSection("JwtSettings");

                var secretKey = jwtSettings["SecretKey"];
                if (string.IsNullOrEmpty(secretKey))
                {
                    _logger.LogError("JWT SecretKey is not configured!");
                    throw new InvalidOperationException("JWT SecretKey not configured");
                }

                var issuer = jwtSettings["Issuer"];
                var audience = jwtSettings["Audience"];
                var expirationMinutes = int.Parse(jwtSettings["ExpirationInMinutes"] ?? "1440");

                _logger.LogDebug("JWT Settings - Issuer: {Issuer}, Audience: {Audience}, Expiration: {ExpirationMinutes} minutes", 
                    issuer, audience, expirationMinutes);

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

                var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
                _logger.LogDebug("JWT token generated successfully");

                return tokenString;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating JWT token for user: {UserId}", userId);
                throw;
            }
        }

        public string HashPassword(string password)
        {
            _logger.LogDebug("Hashing password");
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool VerifyPassword(string password, string passwordHash)
        {
            try
            {
                _logger.LogDebug("Verifying password");
                var result = BCrypt.Net.BCrypt.Verify(password, passwordHash);
                _logger.LogDebug("Password verification result: {Result}", result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying password");
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
