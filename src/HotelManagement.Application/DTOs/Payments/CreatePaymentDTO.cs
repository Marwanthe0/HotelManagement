namespace HotelManagement.Application.DTOs.Payments;

public class CreatePaymentDTO
{
    public int BookingId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
}