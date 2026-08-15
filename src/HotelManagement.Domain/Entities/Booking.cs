namespace HotelManagement.Domain.Entities;

public class Booking
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public int RoomId { get; set; }
    public DateTime CheckInDate{ get; set; }
    public DateTime CheckOutDate{ get; set; }
    public DateTime BookingDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }

    // Navigation Properties
    public Customer Customer { get; set; } = null!;
    public Room Room { get; set; } = null!;
}