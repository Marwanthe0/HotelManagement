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
        return payments.Select(payment => MapToResponseDTO(payment));
    }

    public async Task<PaymentResponseDTO?> GetPaymentByIdAsync(int id)
    {
        var payment = await _paymentRepository.GetByIdAsync(id);
        if (payment is null)
            return null;

        return MapToResponseDTO(payment);
    }

    public async Task<IEnumerable<PaymentResponseDTO>> GetPaymentsByBookingIdAsync(int bookingId)
    {
        var payments = await _paymentRepository.GetByBookingIdAsync(bookingId);
        return payments.Select(payment => MapToResponseDTO(payment));
    }


    public async Task<PaymentResponseDTO> CreatePaymentAsync(CreatePaymentDTO dto)
    {
        // 1. Booking must exist.
        var booking = await _bookingRepository.GetByIdAsync(dto.BookingId);
        if (booking is null)
        {
            throw new InvalidOperationException("Booking with this Id not Found.");
        }

        // 2. Amount must be greater than zero.
        if (dto.Amount <= 0)
        {
            throw new ArgumentException("Payment amount must be greater than Zero.");
        }

        // 3. Payment method is required.
        if (string.IsNullOrWhiteSpace(dto.PaymentMethod))
        {
            throw new ArgumentException("Payment method is required.");
        }

        // 4. A cancelled booking must not accept new payments.
        if (booking.Status == "Cancelled")
        {
            throw new InvalidOperationException(
                "Cannot record a payment for a cancelled booking."
            );
        }

        // 5. Calculate the total already-paid amount using successful payments.
        var totalPaidAmount = await _paymentRepository.GetPaidAmountByBookingIdAsync(
            dto.BookingId
        );

        // 6. New payment cannot exceed the remaining amount.
        var remainingAmount = booking.TotalAmount - totalPaidAmount;

        if (remainingAmount <= 0)
        {
            throw new InvalidOperationException(
                "This booking is already fully paid."
            );
        }

        if (dto.Amount > remainingAmount)
        {
            throw new InvalidOperationException(
                $"Payment Amount cannot exceed the remaining amount of {remainingAmount}."
            );
        }

        var payment = new Payment
        {
            BookingId = dto.BookingId,
            Amount = dto.Amount,
            PaymentMethod = dto.PaymentMethod.Trim(),
            PaymentDate = DateTime.UtcNow,
            PaymentStatus = "Paid",
        };

        var createdPayment = await _paymentRepository.CreateAsync(payment);

        // Auto-confirm: when the booking is fully paid and still Pending,
        // promote it to Confirmed so the guest can check in.
        var updatedPaidAmount = await _paymentRepository.GetPaidAmountByBookingIdAsync(dto.BookingId);
        if (updatedPaidAmount >= booking.TotalAmount && booking.Status == "Pending")
        {
            booking.Status = "Confirmed";
            await _bookingRepository.UpdateAsync(booking);
        }

        return MapToResponseDTO(createdPayment);
    }

    public async Task<PaymentSummaryDTO?> GetPaymentSummaryAsync(int bookingId)
    {
        var booking = await _bookingRepository.GetByIdAsync(bookingId);

        if (booking is null)
            return null;

        var paidAmount = await _paymentRepository.GetPaidAmountByBookingIdAsync(bookingId);

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

    private static PaymentResponseDTO MapToResponseDTO(Payment payment)
    {
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
}


