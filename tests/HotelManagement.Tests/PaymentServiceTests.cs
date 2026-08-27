using HotelManagement.Application.DTOs.Payments;
using HotelManagement.Application.Services;
using HotelManagement.Domain.Entities;
using HotelManagement.Infrastructure.Repositories;

namespace HotelManagement.Tests;

/// <summary>
/// Covers the payment rules: partial payments, full payment, overpayment prevention
/// and the derived booking payment summary (Unpaid / PartiallyPaid / Paid).
/// </summary>
public class PaymentServiceTests
{
    private const decimal BookingTotal = 10000m;

    private sealed record Sut(TestDbContext Db, PaymentService Payments);

    private static async Task<Sut> CreateSutAsync(string bookingStatus = "Confirmed")
    {
        var db = new TestDbContext();

        db.Context.Customers.Add(new Customer
        {
            FirstName = "Marwan",
            LastName = "Rahman",
            Email = "marwan@example.com",
        });
        db.Context.Rooms.Add(new Room
        {
            RoomNumber = "101",
            RoomType = "Deluxe",
            PricePerNight = 2000m,
        });
        await db.Context.SaveChangesAsync();

        db.Context.Bookings.Add(new Booking
        {
            CustomerId = 1,
            RoomId = 1,
            CheckInDate = new DateTime(2026, 6, 10),
            CheckOutDate = new DateTime(2026, 6, 15),
            BookingDate = DateTime.UtcNow,
            Status = bookingStatus,
            TotalAmount = BookingTotal,
        });
        await db.Context.SaveChangesAsync();

        var paymentRepo = new PaymentRepository(db.Context);
        var bookingRepo = new BookingRepository(db.Context);

        return new Sut(db, new PaymentService(paymentRepo, bookingRepo));
    }

    private static CreatePaymentDTO Pay(decimal amount) => new()
    {
        BookingId = 1,
        Amount = amount,
        PaymentMethod = "Card",
    };

    [Fact]
    public async Task Summary_WithNoPayments_IsUnpaid()
    {
        var sut = await CreateSutAsync();
        using (sut.Db)
        {
            var summary = await sut.Payments.GetPaymentSummaryAsync(1);

            Assert.NotNull(summary);
            Assert.Equal(BookingTotal, summary!.TotalAmount);
            Assert.Equal(0m, summary.PaidAmount);
            Assert.Equal(BookingTotal, summary.RemainingAmount);
            Assert.Equal("Unpaid", summary.PaymentStatus);
        }
    }

    [Fact]
    public async Task PartialPayment_ProducesPartiallyPaidSummary()
    {
        var sut = await CreateSutAsync();
        using (sut.Db)
        {
            var payment = await sut.Payments.CreatePaymentAsync(Pay(3000m));

            // An individual successful payment record is always "Paid".
            Assert.Equal("Paid", payment.PaymentStatus);

            var summary = await sut.Payments.GetPaymentSummaryAsync(1);

            Assert.Equal(3000m, summary!.PaidAmount);
            Assert.Equal(7000m, summary.RemainingAmount);
            Assert.Equal("PartiallyPaid", summary.PaymentStatus);
        }
    }

    [Fact]
    public async Task MultiplePayments_TotallingTheBooking_ProducePaidSummary()
    {
        var sut = await CreateSutAsync();
        using (sut.Db)
        {
            await sut.Payments.CreatePaymentAsync(Pay(3000m));
            await sut.Payments.CreatePaymentAsync(Pay(4000m));
            await sut.Payments.CreatePaymentAsync(Pay(3000m));

            var summary = await sut.Payments.GetPaymentSummaryAsync(1);

            Assert.Equal(10000m, summary!.PaidAmount);
            Assert.Equal(0m, summary.RemainingAmount);
            Assert.Equal("Paid", summary.PaymentStatus);

            Assert.Equal(3, (await sut.Payments.GetPaymentsByBookingIdAsync(1)).Count());
        }
    }

    [Fact]
    public async Task Payment_ExceedingRemainingAmount_IsRejected()
    {
        var sut = await CreateSutAsync();
        using (sut.Db)
        {
            await sut.Payments.CreatePaymentAsync(Pay(7000m));

            var error = await Assert.ThrowsAsync<InvalidOperationException>(
                () => sut.Payments.CreatePaymentAsync(Pay(4000m)));

            Assert.Contains("cannot exceed the remaining amount", error.Message);

            // The rejected payment must not have been stored.
            var summary = await sut.Payments.GetPaymentSummaryAsync(1);
            Assert.Equal(7000m, summary!.PaidAmount);
        }
    }

    [Fact]
    public async Task Payment_OnFullyPaidBooking_IsRejected()
    {
        var sut = await CreateSutAsync();
        using (sut.Db)
        {
            await sut.Payments.CreatePaymentAsync(Pay(BookingTotal));

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => sut.Payments.CreatePaymentAsync(Pay(1m)));
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-500)]
    public async Task Payment_WithNonPositiveAmount_IsRejected(decimal amount)
    {
        var sut = await CreateSutAsync();
        using (sut.Db)
        {
            await Assert.ThrowsAsync<ArgumentException>(
                () => sut.Payments.CreatePaymentAsync(Pay(amount)));
        }
    }

    [Fact]
    public async Task Payment_WithoutPaymentMethod_IsRejected()
    {
        var sut = await CreateSutAsync();
        using (sut.Db)
        {
            await Assert.ThrowsAsync<ArgumentException>(
                () => sut.Payments.CreatePaymentAsync(new CreatePaymentDTO
                {
                    BookingId = 1,
                    Amount = 1000m,
                    PaymentMethod = "   ",
                }));
        }
    }

    [Fact]
    public async Task Payment_ForUnknownBooking_IsRejected()
    {
        var sut = await CreateSutAsync();
        using (sut.Db)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => sut.Payments.CreatePaymentAsync(new CreatePaymentDTO
                {
                    BookingId = 999,
                    Amount = 1000m,
                    PaymentMethod = "Cash",
                }));
        }
    }

    [Fact]
    public async Task Payment_OnCancelledBooking_IsRejected()
    {
        var sut = await CreateSutAsync("Cancelled");
        using (sut.Db)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => sut.Payments.CreatePaymentAsync(Pay(1000m)));
        }
    }

    [Fact]
    public async Task Summary_ForUnknownBooking_ReturnsNull()
    {
        var sut = await CreateSutAsync();
        using (sut.Db)
        {
            Assert.Null(await sut.Payments.GetPaymentSummaryAsync(999));
        }
    }

    [Fact]
    public async Task CreatedPayment_UsesUtcDate_AndIsLinkedToBooking()
    {
        var sut = await CreateSutAsync();
        using (sut.Db)
        {
            var before = DateTime.UtcNow.AddSeconds(-5);

            var payment = await sut.Payments.CreatePaymentAsync(Pay(1000m));

            Assert.Equal(1, payment.BookingId);
            Assert.InRange(payment.PaymentDate, before, DateTime.UtcNow.AddSeconds(5));
        }
    }
}
