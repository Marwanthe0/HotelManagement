using HotelManagement.Application.DTOs.Profile;

namespace HotelManagement.Application.Interfaces;

public interface IProfileService
{
    Task<UserProfileDTO> GetProfileAsync(int userId);
    Task<UserProfileDTO> UpdateProfileAsync(int userId, UpdateProfileDTO dto);
    Task<bool> ChangePasswordAsync(int userId, ChangePasswordDTO dto);
}
