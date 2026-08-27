using HotelManagement.Domain.Entities;
using HotelManagement.Infrastructure.Repositories;

namespace HotelManagement.Tests;

/// <summary>
/// Verifies the room availability / overlap rules:
///   overlap  => existing.CheckInDate &lt; requested.CheckOutDate
///            && existing.CheckOutDate &gt; requested.CheckInDate
/// Pending / Confirmed / CheckedIn block a room; Cancelled / CheckedOut do not.
/// </summary>
public class RoomAvailabilityTests
{
    private static readonly DateTime CheckIn = new(2026, 6, 10);
    private static readonly DateTime CheckOut = new(2026, 6, 15);

    private static async Task<(TestDbContext db, BookingRepository repo)> ArrangeAsync(
        string existingBookingStatus,
        DateTime existingCheckIn,
        DateTime existingCheckOut)
    {
        var db = new TestDbContext();

        var customer = new Customer { FirstName = "A", LastName = "B", Email = "a@b.com" };
        var room = new Room { RoomNumber = "101", RoomType = "Deluxe", PricePerNight = 2000m };

        db.Context.Customers.Add(customer);
        db.Context.Rooms.Add(room);
        await db.Context.SaveChangesAsync();

        db.Context.Bookings.Add(new Booking
        {
            CustomerId = customer.Id,
            RoomId = room.Id,
            CheckInDate = existingCheckIn,
            CheckOutDate = existingCheckOut,
            BookingDate = DateTime.UtcNow,
            Status = existingBookingStatus,
            TotalAmount = 10000m,
        });
        await db.Context.SaveChangesAsync();

        return (db, new BookingRepository(db.Context));
    }

    [Theory]
    [InlineData("Pending")]
    [InlineData("Confirmed")]
    [InlineData("CheckedIn")]
    public async Task ActiveBooking_BlocksRoom(string status)
    {
        var (db, repo) = await ArrangeAsync(status, CheckIn, CheckOut);
        using (db)
        {
            var isAvailable = await repo.IsRoomAvailableAsync(1, CheckIn, CheckOut);

            Assert.False(isAvailable);
        }
    }

    [Theory]
    [InlineData("Cancelled")]
    [InlineData("CheckedOut")]
    public async Task CancelledOrCheckedOutBooking_DoesNotBlockRoom(string status)
    {
        var (db, repo) = await ArrangeAsync(status, CheckIn, CheckOut);
        using (db)
        {
            var isAvailable = await repo.IsRoomAvailableAsync(1, CheckIn, CheckOut);

            Assert.True(isAvailable);
        }
    }

    [Fact]
    public async Task NonOverlappingDates_AreAvailable()
    {
        // Existing booking ends exactly when the requested one starts -> no overlap.
        var (db, repo) = await ArrangeAsync("Confirmed", new DateTime(2026, 6, 5), CheckIn);
        using (db)
        {
            var isAvailable = await repo.IsRoomAvailableAsync(1, CheckIn, CheckOut);

            Assert.True(isAvailable);
        }
    }

    [Fact]
    public async Task ExcludedBookingId_IsIgnoredDuringUpdate()
    {
        var (db, repo) = await ArrangeAsync("Confirmed", CheckIn, CheckOut);
        using (db)
        {
            // Without exclusion the booking blocks itself.
            Assert.False(await repo.IsRoomAvailableAsync(1, CheckIn, CheckOut));

            // Excluding the booking being updated makes the same range available.
            Assert.True(await repo.IsRoomAvailableAsync(1, CheckIn, CheckOut, excludedBookingId: 1));
        }
    }

    [Fact]
    public async Task GetBookedRoomIds_ReturnsOnlyBlockedRooms()
    {
        var (db, repo) = await ArrangeAsync("Confirmed", CheckIn, CheckOut);
        using (db)
        {
            // A second, free room must not be reported as booked.
            db.Context.Rooms.Add(new Room
            {
                RoomNumber = "102",
                RoomType = "Standard",
                PricePerNight = 1500m,
            });
            await db.Context.SaveChangesAsync();

            var bookedRoomIds = (await repo.GetBookedRoomIdsAsync(CheckIn, CheckOut)).ToList();

            Assert.Equal(new[] { 1 }, bookedRoomIds);
        }
    }
}
