namespace HotelManagement.Domain.Entities;

public class Customer
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set;} = string.Empty;
    public string Phone { get; set;} = string.Empty;
    public string Address { get; set; } = string.Empty;
    
    // Navigation Property
    public ICollection<Booking> Bookings { get; set; }
        = new List<Booking>();
}