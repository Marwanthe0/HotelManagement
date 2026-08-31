using HotelManagement.Application.DTOs.Profile;
using HotelManagement.Application.Interfaces;
using HotelManagement.Domain.Entities;

namespace HotelManagement.Application.Services;

public class ProfileService : IProfileService
{
    private readonly IUserRepository _userRepository;

    public ProfileService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserProfileDTO> GetProfileAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        return MapToDto(user);
    }

    public async Task<UserProfileDTO> UpdateProfileAsync(int userId, UpdateProfileDTO dto)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        var trimmedUsername = dto.Username.Trim();
        var usernameExists = await _userRepository.ExistsByUsernameAsync(trimmedUsername, userId);
        if (usernameExists)
        {
            throw new InvalidOperationException("Username is already taken.");
        }

        user.Username = trimmedUsername;
        user.PhoneNumber = dto.PhoneNumber?.Trim() ?? string.Empty;
        user.Address = dto.Address?.Trim() ?? string.Empty;

        if (dto.ProfilePictureUrl is not null)
        {
            user.ProfilePictureUrl = dto.ProfilePictureUrl;
        }

        await _userRepository.UpdateAsync(user);

        return MapToDto(user);
    }

    public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordDTO dto)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
        {
            throw new InvalidOperationException("Current password is incorrect.");
        }

        if (dto.NewPassword != dto.ConfirmNewPassword)
        {
            throw new InvalidOperationException("New password and confirmation do not match.");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        await _userRepository.UpdateAsync(user);

        return true;
    }

    private static UserProfileDTO MapToDto(User user)
    {
        return new UserProfileDTO
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role,
            PhoneNumber = user.PhoneNumber,
            Address = user.Address,
            ProfilePictureUrl = user.ProfilePictureUrl,
            CreatedAt = user.CreatedAt
        };
    }
}
