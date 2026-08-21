namespace HotelManagement.Application.DTOs.Bookings;

public class UpdateBookingDTO
{
    public int RoomId { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
}