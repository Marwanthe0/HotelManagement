using HotelManagement.Application.Interfaces;
using HotelManagement.Domain.Entities;
using HotelManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Infrastructure.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly HotelDbContext _context;

    public PaymentRepository(HotelDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Payment>> GetAllAsync()
    {
        return await _context.Payments.ToListAsync();
    }

    public async Task<Payment?> GetByIdAsync(int id)
    {
        return await _context.Payments.FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Payment> CreateAsync(Payment payment)
    {
        await _context.Payments.AddAsync(payment);
        await _context.SaveChangesAsync();
        return payment;
    }

    public async Task<IEnumerable<Payment>> GetByBookingIdAsync(int bookingId)
    {
        return await _context
            .Payments.Where(p => p.BookingId == bookingId)
            .OrderBy(p => p.PaymentDate)
            .ToListAsync();
    }

    public async Task<decimal> GetPaidAmountByBookingIdAsync(int bookingId)
    {
        return await _context
            .Payments.Where(p => p.BookingId == bookingId && p.PaymentStatus == "Paid")
            .SumAsync(p => (decimal?)p.Amount) ?? 0m;
    }

    public async Task<bool> HasPaymentsForBookingAsync(int bookingId)
    {
        return await _context.Payments.AnyAsync(p => p.BookingId == bookingId);
    }
}


