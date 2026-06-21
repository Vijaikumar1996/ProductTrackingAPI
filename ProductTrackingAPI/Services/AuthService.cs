using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ProductTrackingAPI.Data;
using ProductTrackingAPI.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ProductTrackingAPI.DTOs;
using System.ComponentModel.DataAnnotations;


namespace ProductTrackingAPI.Api.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(
        ApplicationDbContext context,
        IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<LoginResponseDto?> LoginAsync(
     LoginRequestDto request)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(x =>
                x.Email == request.Email &&
                x.IsActive);

        if (user == null)
        {
            throw new ValidationException("Invalid email or password.");
        }

        // TEMPORARY FOR MVP
        if (user.PasswordHash != request.Password)
        {
            throw new ValidationException("Invalid email or password.");
        }

        // Application Access Validation
        if (request.AppType == "WEB" &&
            !user.CanAccessWeb)
        {
            throw new ValidationException(
                "You are not authorized to access Web Portal.");
        }

        if (request.AppType == "SCANNER" &&
            !user.CanAccessScanner)
        {
            throw new ValidationException(
                "You are not authorized to access Scanner App.");
        }

        var roles = user.UserRoles
            .Select(x => x.Role.Code)
            .ToList();

        if (!roles.Any())
        {
            throw new ValidationException(
                "No roles assigned to this user. Please contact administrator.");
        }

        var claims = new List<Claim>
    {
        new Claim(
            ClaimTypes.NameIdentifier,
            user.Id.ToString()),

        new Claim(
            ClaimTypes.Email,
            user.Email)
    };

        foreach (var role in roles)
        {
            claims.Add(
                new Claim(
                    ClaimTypes.Role,
                    role));
        }

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(
                _configuration["Jwt:Key"]!));

        var credentials =
            new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

        var token =
            new JwtSecurityToken(
                issuer:
                    _configuration["Jwt:Issuer"],
                audience:
                    _configuration["Jwt:Audience"],
                claims: claims,
                expires:
                    DateTime.UtcNow.AddHours(12),
                signingCredentials:
                    credentials);

        return new LoginResponseDto
        {
            AccessToken =
                new JwtSecurityTokenHandler()
                    .WriteToken(token),

            Name =
                user.Name,

            Email =
                user.Email,

            Roles =
                roles
        };
    }


    public async Task<string> ChangePasswordAsync(
    ChangePasswordRequesDto request,
    ClaimsPrincipal userClaims)
    {
        if (string.IsNullOrWhiteSpace(
            request.CurrentPassword))
        {
            throw new Exception(
                "Current password is required");
        }

        if (string.IsNullOrWhiteSpace(
            request.NewPassword))
        {
            throw new Exception(
                "New password is required");
        }

        if (request.NewPassword.Length < 6)
        {
            throw new Exception(
                "New password must be at least 6 characters");
        }

        if (request.CurrentPassword ==
            request.NewPassword)
        {
            throw new Exception(
                "New password must be different from current password");
        }

        var userId = long.Parse(
            userClaims.FindFirst(
                ClaimTypes.NameIdentifier)!
            .Value);

        var user = await _context.Users
            .FirstOrDefaultAsync(x =>
                x.Id == userId);

        if (user == null)
        {
            throw new Exception(
                "User not found");
        }

        if (user.PasswordHash !=
            request.CurrentPassword)
        {
            throw new Exception(
                "Current password is incorrect");
        }

        user.PasswordHash =
            request.NewPassword;

        await _context.SaveChangesAsync();

        return "Password changed successfully";
    }
}