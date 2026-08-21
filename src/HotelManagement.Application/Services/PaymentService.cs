using HotelManagement.Application.DTOs.Payments;
using HotelManagement.Application.Interfaces;
using HotelManagement.Domain.Entities;

namespace HotelManagement.Application.Services;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IBookingRepository _bookingRepository;

    public PaymentService(
        IPaymentRepository paymentRepository,
        IBookingRepository bookingRepository
    )
    {
        _paymentRepository = paymentRepository;
        _bookingRepository = bookingRepository;
    }

    public async Task<IEnumerable<PaymentResponseDTO>> GetAllPaymentsAsync()
    {
        var payments = await _paymentRepository.GetAllAsync();
        return payments.Select(payment => new PaymentResponseDTO
        {
            Id = payment.Id,
            BookingId = payment.BookingId,
            Amount = payment.Amount,
            PaymentDate = payment.PaymentDate,
            PaymentMethod = payment.PaymentMethod,
            PaymentStatus = payment.PaymentStatus,
        });
    }

    public async Task<PaymentResponseDTO?> GetPaymentByIdAsync(int id)
    {
        var payment = await _paymentRepository.GetByIdAsync(id);
        if (payment is null)
            return null;

        return new PaymentResponseDTO
        {
            Id = payment.Id,
            BookingId = payment.BookingId,
            Amount = payment.Amount,
            PaymentDate = payment.PaymentDate,
            PaymentMethod = payment.PaymentMethod,
            PaymentStatus = payment.PaymentStatus,
        };
    }

    public async Task<IEnumerable<PaymentResponseDTO>> GetPaymentsByBookingIdAsync(int bookingId)
    {
        var payments = await _paymentRepository.GetByBookingIdAsync(bookingId);
        return payments.Select(payment => new PaymentResponseDTO
        {
            Id = payment.Id,
            BookingId = payment.BookingId,
            Amount = payment.Amount,
            PaymentDate = payment.PaymentDate,
            PaymentMethod = payment.PaymentMethod,
            PaymentStatus = payment.PaymentStatus,
        });
    }

    public async Task<PaymentResponseDTO> CreatePaymentAsync(CreatePaymentDTO dto)
    {
        var booking = await _bookingRepository.GetByIdAsync(dto.BookingId);
        if (booking is null)
        {
            throw new InvalidOperationException("Booking with this Id not Found.");
        }
        if (dto.Amount <= 0)
        {
            throw new ArgumentException("Payment amount must be greater than Zero.");
        }

        var existingPayments = await _paymentRepository.GetByBookingIdAsync(dto.BookingId);
        var totalPaidAmount = existingPayments
            .Where(p => p.PaymentStatus == "Paid")
            .Sum(p => p.Amount);

        var remainingAmount = booking.TotalAmount - totalPaidAmount;

        if (dto.Amount > remainingAmount)
        {
            throw new InvalidOperationException(
                $"Payment Amount cannot exceed the remaining amount of {remainingAmount}."
            );
        }

        if (string.IsNullOrWhiteSpace(dto.PaymentMethod))
        {
            throw new ArgumentException("Payment method is required.");
        }
        var payment = new Payment
        {
            BookingId = dto.BookingId,
            Amount = dto.Amount,
            PaymentMethod = dto.PaymentMethod,
            PaymentDate = DateTime.UtcNow,
            PaymentStatus = "Paid",
        };
        var CreatedPayment = await _paymentRepository.CreateAsync(payment);

        return new PaymentResponseDTO
        {
            Id = CreatedPayment.Id,
            BookingId = CreatedPayment.BookingId,
            Amount = CreatedPayment.Amount,
            PaymentDate = CreatedPayment.PaymentDate,
            PaymentMethod = CreatedPayment.PaymentMethod,
            PaymentStatus = CreatedPayment.PaymentStatus,
        };
    }

    public async Task<PaymentSummaryDTO?> GetPaymentSummaryAsync(int bookingId)
    {
        var booking = await _bookingRepository.GetByIdAsync(bookingId);

        if (booking is null)
            return null;
        var payments = await _paymentRepository.GetByBookingIdAsync(bookingId);

        var paidAmount = payments.Where(p => p.PaymentStatus == "Paid").Sum(p => p.Amount);

        var remainingAmount = booking.TotalAmount - paidAmount;
        string paymentStatus;

        if (paidAmount == 0)
            paymentStatus = "Unpaid";
        else if (remainingAmount > 0)
            paymentStatus = "PartiallyPaid";
        else
            paymentStatus = "Paid";

        return new PaymentSummaryDTO
        {
            BookingId = booking.Id,
            TotalAmount = booking.TotalAmount,
            PaidAmount = paidAmount,
            RemainingAmount = remainingAmount,
            PaymentStatus = paymentStatus,
        };
    }
}
