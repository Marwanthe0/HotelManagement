using HotelManagement.Application.DTOs.Auth;

namespace HotelManagement.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDTO> LoginAsync(LoginDTO dto);
    Task<AuthResponseDTO> RegisterAsync(RegisterDTO dto);
}
