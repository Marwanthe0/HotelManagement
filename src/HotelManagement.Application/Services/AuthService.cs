using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HotelManagement.Application.DTOs.Auth;
using HotelManagement.Application.Interfaces;
using HotelManagement.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace HotelManagement.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _config;

    public AuthService(IUserRepository userRepository, IConfiguration config)
    {
        _userRepository = userRepository;
        _config = config;
    }

    public async Task<AuthResponseDTO> LoginAsync(LoginDTO dto)
    {
        var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
        var user = await _userRepository.GetByEmailAsync(normalizedEmail);

        if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        return GenerateToken(user);
    }

    public async Task<AuthResponseDTO> RegisterAsync(RegisterDTO dto)
    {
        var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
        var emailExists = await _userRepository.ExistsByEmailAsync(normalizedEmail);
        if (emailExists)
        {
            throw new InvalidOperationException("A user with this email already exists.");
        }

        var usernameExists = await _userRepository.ExistsByUsernameAsync(dto.Username.Trim());
        if (usernameExists)
        {
            throw new InvalidOperationException("A user with this username already exists.");
        }

        var user = new User
        {
            Username = dto.Username.Trim(),
            Email = normalizedEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = string.IsNullOrWhiteSpace(dto.Role) ? "Staff" : dto.Role.Trim(),
            PhoneNumber = dto.PhoneNumber?.Trim() ?? string.Empty,
            Address = dto.Address?.Trim() ?? string.Empty,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user);
        return GenerateToken(user);
    }

    private AuthResponseDTO GenerateToken(User user)
    {
        var jwtKey = _config["Jwt:Key"] ?? "HotelManagement_Super_Secret_Key_At_Least_32_Chars!";
        var jwtIssuer = _config["Jwt:Issuer"] ?? "HotelManagementAPI";
        var jwtAudience = _config["Jwt:Audience"] ?? "HotelManagementClient";
        var expiresAt = DateTime.UtcNow.AddHours(8);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials
        );

        var token = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);

        return new AuthResponseDTO
        {
            Token = token,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role,
            PhoneNumber = user.PhoneNumber,
            Address = user.Address,
            ProfilePictureUrl = user.ProfilePictureUrl,
            ExpiresAt = expiresAt
        };
    }
}
