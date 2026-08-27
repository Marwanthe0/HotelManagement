using System.ComponentModel.DataAnnotations;

namespace HotelManagement.Application.DTOs.Bookings;

public class UpdateBookingDTO
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "RoomId must be a positive number.")]
    public int RoomId { get; set; }


    [Required]
    public DateTime CheckInDate { get; set; }

    [Required]
    public DateTime CheckOutDate { get; set; }
}