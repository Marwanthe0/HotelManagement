namespace HotelManagement.Application.DTOs.Payments;

public class PaymentSummaryDTO
{
    public int BookingId { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
}
