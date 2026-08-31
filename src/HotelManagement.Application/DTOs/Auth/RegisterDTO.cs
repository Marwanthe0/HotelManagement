using System.ComponentModel.DataAnnotations;

namespace HotelManagement.Application.DTOs.Auth;

public class RegisterDTO
{
    [Required]
    [StringLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
    public string Password { get; set; } = string.Empty;

    [StringLength(20)]
    public string Role { get; set; } = "Staff";

    [StringLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;

    [StringLength(200)]
    public string Address { get; set; } = string.Empty;
}
