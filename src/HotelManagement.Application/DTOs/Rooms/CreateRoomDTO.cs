using System.ComponentModel.DataAnnotations;
namespace HotelManagement.Application.DTOs.Rooms;

public class CreateRoomDTO
{
    [Required]
    [StringLength(20)]
    public string RoomNumber { get; set; } = string.Empty;
    [Required]
    [StringLength(20)]
    public string RoomType { get; set; } = string.Empty;
    [Range(0.01,1000000)]
    public decimal PricePerNight { get; set; }
    public bool IsAvailable { get; set; }
}