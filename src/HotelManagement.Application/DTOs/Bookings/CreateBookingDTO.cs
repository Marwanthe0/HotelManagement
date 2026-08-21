namespace HotelManagement.Application.DTOs;

public class CreateBookingDTO
{
    public int CustomerId { get; set; }
    public int RoomId { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
}