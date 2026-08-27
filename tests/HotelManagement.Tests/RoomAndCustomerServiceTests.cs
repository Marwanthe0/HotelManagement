using HotelManagement.Application.DTOs.Customers;
using HotelManagement.Application.DTOs.Rooms;
using HotelManagement.Application.Services;
using HotelManagement.Domain.Entities;
using HotelManagement.Infrastructure.Repositories;

namespace HotelManagement.Tests;

/// <summary>
/// Covers unique RoomNumber enforcement, unique customer email,
/// safe-delete behaviour and the availability endpoint's date validation.
/// </summary>
public class RoomAndCustomerServiceTests
{
    private sealed record Sut(
        TestDbContext Db,
        RoomService Rooms,
        CustomerService Customers,
        BookingService Bookings);

    private static Sut CreateSut()
    {
        var db = new TestDbContext();

        var bookingRepo = new BookingRepository(db.Context);
        var roomRepo = new RoomRepository(db.Context);
        var customerRepo = new CustomerRepository(db.Context);
        var paymentRepo = new PaymentRepository(db.Context);

        return new Sut(
            db,
            new RoomService(roomRepo, bookingRepo),
            new CustomerService(customerRepo, bookingRepo),
            new BookingService(bookingRepo, customerRepo, roomRepo, paymentRepo));
    }

    private static CreateRoomDTO NewRoom(string roomNumber = "101") => new()
    {
        RoomNumber = roomNumber,
        RoomType = "Deluxe",
        PricePerNight = 2000m,
        IsAvailable = true,
    };

    private static CreateCustomerDTO NewCustomer(string email = "marwan@example.com") => new()
    {
        FirstName = "Marwan",
        LastName = "Rahman",
        Email = email,
        Phone = "0123456789",
        Address = "Dhaka",
    };

    [Fact]
    public async Task DuplicateRoomNumber_OnCreate_IsRejected()
    {
        var sut = CreateSut();
        using (sut.Db)
        {
            await sut.Rooms.AddRoomAsync(NewRoom());

            var error = await Assert.ThrowsAsync<InvalidOperationException>(
                () => sut.Rooms.AddRoomAsync(NewRoom()));

            Assert.Contains("already exists", error.Message);
        }
    }

    [Fact]
    public async Task DuplicateRoomNumber_OnUpdate_IsRejected()
    {
        var sut = CreateSut();
        using (sut.Db)
        {
            await sut.Rooms.AddRoomAsync(NewRoom("101"));
            var second = await sut.Rooms.AddRoomAsync(NewRoom("102"));

            // Renaming room 102 to 101 must be rejected.
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => sut.Rooms.UpdateRoomAsync(second.Id, new UpdateRoomDto
                {
                    RoomNumber = "101",
                    RoomType = "Deluxe",
                    PricePerNight = 2500m,
                    IsAvailable = true,
                }));
        }
    }

    [Fact]
    public async Task UpdatingRoom_KeepingItsOwnRoomNumber_IsAllowed()
    {
        var sut = CreateSut();
        using (sut.Db)
        {
            var room = await sut.Rooms.AddRoomAsync(NewRoom("101"));

            var updated = await sut.Rooms.UpdateRoomAsync(room.Id, new UpdateRoomDto
            {
                RoomNumber = "101",
                RoomType = "Suite",
                PricePerNight = 3000m,
                IsAvailable = true,
            });

            Assert.Equal("Suite", updated!.RoomType);
            Assert.Equal(3000m, updated.PricePerNight);
        }
    }

    [Fact]
    public async Task DeletingRoom_WithActiveBooking_IsRejected()
    {
        var sut = CreateSut();
        using (sut.Db)
        {
            var room = await sut.Rooms.AddRoomAsync(NewRoom());
            var customer = await sut.Customers.CreateAsync(NewCustomer());

            await sut.Bookings.CreateAsync(new Application.DTOs.Bookings.CreateBookingDTO
            {
                CustomerId = customer.Id,
                RoomId = room.Id,
                CheckInDate = new DateTime(2026, 6, 10),
                CheckOutDate = new DateTime(2026, 6, 15),
            });

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => sut.Rooms.DeleteRoomAsync(room.Id));
        }
    }

    [Fact]
    public async Task AvailableRooms_RequireAValidDateRange()
    {
        var sut = CreateSut();
        using (sut.Db)
        {
            // Missing query parameters bind to default(DateTime).
            await Assert.ThrowsAsync<ArgumentException>(
                () => sut.Rooms.GetAvailableRoomsAsync(default, default));

            // Check-out before check-in.
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => sut.Rooms.GetAvailableRoomsAsync(
                    new DateTime(2026, 6, 15), new DateTime(2026, 6, 10)));
        }
    }

    [Fact]
    public async Task AvailableRooms_ExcludeRoomsBlockedByActiveBookings()
    {
        var sut = CreateSut();
        using (sut.Db)
        {
            var booked = await sut.Rooms.AddRoomAsync(NewRoom("101"));
            await sut.Rooms.AddRoomAsync(NewRoom("102"));
            var customer = await sut.Customers.CreateAsync(NewCustomer());

            await sut.Bookings.CreateAsync(new Application.DTOs.Bookings.CreateBookingDTO
            {
                CustomerId = customer.Id,
                RoomId = booked.Id,
                CheckInDate = new DateTime(2026, 6, 10),
                CheckOutDate = new DateTime(2026, 6, 15),
            });

            var available = (await sut.Rooms.GetAvailableRoomsAsync(
                new DateTime(2026, 6, 10), new DateTime(2026, 6, 15))).ToList();

            Assert.Single(available);
            Assert.Equal("102", available[0].RoomNumber);
        }
    }

    [Fact]
    public async Task DuplicateCustomerEmail_IsRejected()
    {
        var sut = CreateSut();
        using (sut.Db)
        {
            await sut.Customers.CreateAsync(NewCustomer());

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => sut.Customers.CreateAsync(NewCustomer()));
        }
    }

    [Fact]
    public async Task UpdatingCustomer_KeepingItsOwnEmail_IsAllowed()
    {
        var sut = CreateSut();
        using (sut.Db)
        {
            var customer = await sut.Customers.CreateAsync(NewCustomer());

            var updated = await sut.Customers.UpdateAsync(customer.Id, new UpdateCustomerDTO
            {
                FirstName = "Updated",
                LastName = "Name",
                Email = customer.Email,
                Phone = "0987654321",
                Address = "Chittagong",
            });

            Assert.Equal("Updated", updated!.FirstName);
        }
    }

    [Fact]
    public async Task DeletingCustomer_WithBookings_IsRejected()
    {
        var sut = CreateSut();
        using (sut.Db)
        {
            var room = await sut.Rooms.AddRoomAsync(NewRoom());
            var customer = await sut.Customers.CreateAsync(NewCustomer());

            await sut.Bookings.CreateAsync(new Application.DTOs.Bookings.CreateBookingDTO
            {
                CustomerId = customer.Id,
                RoomId = room.Id,
                CheckInDate = new DateTime(2026, 6, 10),
                CheckOutDate = new DateTime(2026, 6, 15),
            });

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => sut.Customers.DeleteAsync(customer.Id));
        }
    }

    [Fact]
    public async Task CustomerSearch_MatchesNameAndEmail()
    {
        var sut = CreateSut();
        using (sut.Db)
        {
            await sut.Customers.CreateAsync(NewCustomer("marwan@example.com"));
            await sut.Customers.CreateAsync(new CreateCustomerDTO
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                Phone = "0111111111",
            });

            Assert.Single(await sut.Customers.GetAllAsync("John"));
            Assert.Single(await sut.Customers.GetAllAsync("marwan@example.com"));
            Assert.Equal(2, (await sut.Customers.GetAllAsync()).Count());
        }
    }

    [Fact]
    public async Task CustomerBookingHistory_ReturnsOnlyThatCustomersBookings()
    {
        var sut = CreateSut();
        using (sut.Db)
        {
            var room = await sut.Rooms.AddRoomAsync(NewRoom());
            var customer = await sut.Customers.CreateAsync(NewCustomer());
            var other = await sut.Customers.CreateAsync(NewCustomer("other@example.com"));

            await sut.Bookings.CreateAsync(new Application.DTOs.Bookings.CreateBookingDTO
            {
                CustomerId = customer.Id,
                RoomId = room.Id,
                CheckInDate = new DateTime(2026, 6, 10),
                CheckOutDate = new DateTime(2026, 6, 15),
            });

            Assert.Single(await sut.Bookings.GetByCustomerIdAsync(customer.Id));
            Assert.Empty(await sut.Bookings.GetByCustomerIdAsync(other.Id));
        }
    }
}
