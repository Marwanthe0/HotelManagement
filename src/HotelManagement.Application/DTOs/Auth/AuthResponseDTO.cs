namespace HotelManagement.Application.DTOs.Auth;

public class AuthResponseDTO
{
    public string Token { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; }
    public DateTime ExpiresAt { get; set; }
}
