using System.ComponentModel.DataAnnotations;

namespace HotelManagement.Application.DTOs.Profile;

public class UpdateProfileDTO
{
    [Required]
    [StringLength(50)]
    public string Username { get; set; } = string.Empty;

    [StringLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;

    [StringLength(200)]
    public string Address { get; set; } = string.Empty;

    public string? ProfilePictureUrl { get; set; }
}
