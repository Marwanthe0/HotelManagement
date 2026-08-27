using System.ComponentModel.DataAnnotations;

namespace HotelManagement.Application.DTOs.Bookings;

public class CreateBookingDTO
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "CustomerId must be a positive number.")]
    public int CustomerId { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "RoomId must be a positive number.")]
    public int RoomId { get; set; }


    [Required]
    public DateTime CheckInDate { get; set; }

    [Required]
    public DateTime CheckOutDate { get; set; }
}