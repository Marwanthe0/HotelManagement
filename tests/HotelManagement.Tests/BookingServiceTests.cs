using HotelManagement.Application.DTOs.Bookings;
using HotelManagement.Application.Services;
using HotelManagement.Domain.Entities;
using HotelManagement.Infrastructure.Repositories;

namespace HotelManagement.Tests;

/// <summary>
/// Covers booking creation validation, server-side total calculation,
/// overlap rejection and the booking status workflow.
/// </summary>
public class BookingServiceTests
{
    private const decimal PricePerNight = 2000m;

    private static readonly DateTime CheckIn = new(2026, 6, 10);
    private static readonly DateTime CheckOut = new(2026, 6, 15); // 5 nights

    private sealed record Sut(
        TestDbContext Db,
        BookingService Bookings,
        PaymentService Payments);

    private static async Task<Sut> CreateSutAsync()
    {
        var db = new TestDbContext();

        db.Context.Customers.Add(new Customer
        {
            FirstName = "Marwan",
            LastName = "Rahman",
            Email = "marwan@example.com",
            Phone = "0123456789",
        });
        db.Context.Rooms.Add(new Room
        {
            RoomNumber = "101",
            RoomType = "Deluxe",
            PricePerNight = PricePerNight,
            IsAvailable = true,
        });
        await db.Context.SaveChangesAsync();

        var bookingRepo = new BookingRepository(db.Context);
        var customerRepo = new CustomerRepository(db.Context);
        var roomRepo = new RoomRepository(db.Context);
        var paymentRepo = new PaymentRepository(db.Context);

        return new Sut(
            db,
            new BookingService(bookingRepo, customerRepo, roomRepo, paymentRepo),
            new PaymentService(paymentRepo, bookingRepo));
    }

    private static CreateBookingDTO NewBooking() => new()
    {
        CustomerId = 1,
        RoomId = 1,
        CheckInDate = CheckIn,
        CheckOutDate = CheckOut,
    };

    [Fact]
    public async Task Create_CalculatesTotalOnServer_AndStartsAsPending()
    {
        var sut = await CreateSutAsync();
        using (sut.Db)
        {
            var booking = await sut.Bookings.CreateAsync(NewBooking());

            Assert.Equal("Pending", booking.Status);
            Assert.Equal(5 * PricePerNight, booking.TotalAmount);
            Assert.NotEqual(default, booking.BookingDate);
        }
    }

    [Fact]
    public async Task Create_WithCheckOutBeforeCheckIn_IsRejected()
    {
        var sut = await CreateSutAsync();
        using (sut.Db)
        {
            var dto = NewBooking();
            dto.CheckInDate = CheckOut;
            dto.CheckOutDate = CheckIn;

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => sut.Bookings.CreateAsync(dto));
        }
    }

    [Fact]
    public async Task Create_WithUnknownCustomerOrRoom_IsRejected()
    {
        var sut = await CreateSutAsync();
        using (sut.Db)
        {
            var noCustomer = NewBooking();
            noCustomer.CustomerId = 999;
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => sut.Bookings.CreateAsync(noCustomer));

            var noRoom = NewBooking();
            noRoom.RoomId = 999;
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => sut.Bookings.CreateAsync(noRoom));
        }
    }

    [Fact]
    public async Task Create_OverlappingBooking_IsRejected()
    {
        var sut = await CreateSutAsync();
        using (sut.Db)
        {
            await sut.Bookings.CreateAsync(NewBooking());

            var overlapping = NewBooking();
            overlapping.CheckInDate = CheckIn.AddDays(2);
            overlapping.CheckOutDate = CheckOut.AddDays(2);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => sut.Bookings.CreateAsync(overlapping));
        }
    }

    [Fact]
    public async Task CancelledBooking_FreesTheRoomForNewBookings()
    {
        var sut = await CreateSutAsync();
        using (sut.Db)
        {
            var first = await sut.Bookings.CreateAsync(NewBooking());
            await sut.Bookings.CancelAsync(first.Id);

            // The same dates must now be bookable again.
            var second = await sut.Bookings.CreateAsync(NewBooking());

            Assert.Equal("Pending", second.Status);
        }
    }

    [Fact]
    public async Task FullLifecycle_PendingToCheckedOut_Succeeds()
    {
        var sut = await CreateSutAsync();
        using (sut.Db)
        {
            var booking = await sut.Bookings.CreateAsync(NewBooking());

            Assert.Equal("Confirmed", (await sut.Bookings.ConfirmAsync(booking.Id))!.Status);
            Assert.Equal("CheckedIn", (await sut.Bookings.CheckInAsync(booking.Id))!.Status);

            // Check-out requires the balance to be settled first.
            await sut.Payments.CreatePaymentAsync(new Application.DTOs.Payments.CreatePaymentDTO
            {
                BookingId = booking.Id,
                Amount = booking.TotalAmount,
                PaymentMethod = "Cash",
            });

            Assert.Equal("CheckedOut", (await sut.Bookings.CheckOutAsync(booking.Id))!.Status);
        }
    }

    [Fact]
    public async Task InvalidStatusTransitions_AreRejected()
    {
        var sut = await CreateSutAsync();
        using (sut.Db)
        {
            var booking = await sut.Bookings.CreateAsync(NewBooking());

            // Pending -> CheckedIn / CheckedOut are invalid.
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => sut.Bookings.CheckInAsync(booking.Id));
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => sut.Bookings.CheckOutAsync(booking.Id));

            await sut.Bookings.CancelAsync(booking.Id);

            // Cancelled -> Confirmed / CheckedIn are invalid.
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => sut.Bookings.ConfirmAsync(booking.Id));
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => sut.Bookings.CheckInAsync(booking.Id));
        }
    }

    [Fact]
    public async Task Confirm_IsOnlyAllowedOnce()
    {
        var sut = await CreateSutAsync();
        using (sut.Db)
        {
            var booking = await sut.Bookings.CreateAsync(NewBooking());
            await sut.Bookings.ConfirmAsync(booking.Id);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => sut.Bookings.ConfirmAsync(booking.Id));
        }
    }

    [Fact]
    public async Task CheckOut_WithOutstandingBalance_IsRejected()
    {
        var sut = await CreateSutAsync();
        using (sut.Db)
        {
            var booking = await sut.Bookings.CreateAsync(NewBooking());
            await sut.Bookings.ConfirmAsync(booking.Id);
            await sut.Bookings.CheckInAsync(booking.Id);

            await sut.Payments.CreatePaymentAsync(new Application.DTOs.Payments.CreatePaymentDTO
            {
                BookingId = booking.Id,
                Amount = 3000m,
                PaymentMethod = "Card",
            });

            var error = await Assert.ThrowsAsync<InvalidOperationException>(
                () => sut.Bookings.CheckOutAsync(booking.Id));

            Assert.Contains("Remaining amount", error.Message);
        }
    }

    [Fact]
    public async Task Update_RecalculatesTotalFromRoomPrice()
    {
        var sut = await CreateSutAsync();
        using (sut.Db)
        {
            var booking = await sut.Bookings.CreateAsync(NewBooking());

            var updated = await sut.Bookings.UpdateAsync(booking.Id, new UpdateBookingDTO
            {
                RoomId = 1,
                CheckInDate = CheckIn,
                CheckOutDate = CheckIn.AddDays(2), // 2 nights
            });

            Assert.Equal(2 * PricePerNight, updated!.TotalAmount);
        }
    }

    [Fact]
    public async Task Update_OnCheckedOutBooking_IsRejected()
    {
        var sut = await CreateSutAsync();
        using (sut.Db)
        {
            var booking = await sut.Bookings.CreateAsync(NewBooking());
            await sut.Bookings.ConfirmAsync(booking.Id);
            await sut.Bookings.CheckInAsync(booking.Id);
            await sut.Payments.CreatePaymentAsync(new Application.DTOs.Payments.CreatePaymentDTO
            {
                BookingId = booking.Id,
                Amount = booking.TotalAmount,
                PaymentMethod = "Cash",
            });
            await sut.Bookings.CheckOutAsync(booking.Id);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => sut.Bookings.UpdateAsync(booking.Id, new UpdateBookingDTO
                {
                    RoomId = 1,
                    CheckInDate = CheckIn,
                    CheckOutDate = CheckOut,
                }));
        }
    }

    [Fact]
    public async Task Delete_BookingWithPayments_IsRejected()
    {
        var sut = await CreateSutAsync();
        using (sut.Db)
        {
            var booking = await sut.Bookings.CreateAsync(NewBooking());
            await sut.Payments.CreatePaymentAsync(new Application.DTOs.Payments.CreatePaymentDTO
            {
                BookingId = booking.Id,
                Amount = 1000m,
                PaymentMethod = "Cash",
            });

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => sut.Bookings.DeleteAsync(booking.Id));
        }
    }

    [Fact]
    public async Task GetAll_FiltersByStatus()
    {
        var sut = await CreateSutAsync();
        using (sut.Db)
        {
            var booking = await sut.Bookings.CreateAsync(NewBooking());
            await sut.Bookings.ConfirmAsync(booking.Id);

            Assert.Single(await sut.Bookings.GetAllAsync("Confirmed"));
            Assert.Empty(await sut.Bookings.GetAllAsync("Pending"));

            // Filter is case-insensitive.
            Assert.Single(await sut.Bookings.GetAllAsync("confirmed"));
        }
    }
}
