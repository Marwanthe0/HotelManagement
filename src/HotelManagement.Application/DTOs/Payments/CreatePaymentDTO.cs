using System.ComponentModel.DataAnnotations;

namespace HotelManagement.Application.DTOs.Payments;

public class CreatePaymentDTO
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "BookingId must be a positive number.")]
    public int BookingId { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
    public decimal Amount { get; set; }

    [Required]
    [StringLength(30)]
    public string PaymentMethod { get; set; } = string.Empty;

}