using HotelManagement.Application.DTOs.Dashboard;
using HotelManagement.Application.Interfaces;

namespace HotelManagement.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IRoomRepository _roomRepo;
    private readonly IBookingRepository _bookingRepo;
    private readonly IPaymentRepository _paymentRepo;
    private readonly ICustomerRepository _customerRepo;

    public DashboardService(
        IRoomRepository roomRepo,
        IBookingRepository bookingRepo,
        IPaymentRepository paymentRepo,
        ICustomerRepository customerRepo)
    {
        _roomRepo = roomRepo;
        _bookingRepo = bookingRepo;
        _paymentRepo = paymentRepo;
        _customerRepo = customerRepo;
    }

    public async Task<DashboardResponseDTO> GetDashboardAsync()
    {
        var rooms = (await _roomRepo.GetAllAsync()).ToList();
        var bookings = (await _bookingRepo.GetAllAsync()).ToList();
        var payments = (await _paymentRepo.GetAllAsync()).ToList();
        var customers = (await _customerRepo.GetAllAsync()).ToList();

        var today = DateTime.UtcNow.Date;

        // Currently occupied rooms (CheckedIn bookings)
        var checkedInBookings = bookings
            .Where(b => b.Status == "CheckedIn")
            .ToList();
        var occupiedRoomIds = checkedInBookings
            .Select(b => b.RoomId)
            .Distinct()
            .ToHashSet();

        // Reserved rooms (Pending or Confirmed covering today)
        var reservedRoomIds = bookings
            .Where(b => (b.Status == "Pending" || b.Status == "Confirmed")
                        && b.CheckInDate.Date <= today
                        && b.CheckOutDate.Date > today)
            .Select(b => b.RoomId)
            .Distinct()
            .Where(id => !occupiedRoomIds.Contains(id))
            .ToHashSet();

        int totalRooms = rooms.Count;
        int occupiedCount = occupiedRoomIds.Count;
        int reservedCount = reservedRoomIds.Count;
        int availableCount = totalRooms - occupiedCount - reservedCount;
        if (availableCount < 0) availableCount = 0;

        // Active bookings
        int activeBookings = bookings.Count(b =>
            b.Status == "Pending" || b.Status == "Confirmed" || b.Status == "CheckedIn");

        // Total revenue from successful payments
        var paidPayments = payments.Where(p => p.PaymentStatus == "Paid").ToList();
        decimal totalRevenue = paidPayments.Sum(p => p.Amount);

        // Outstanding amount across all active reservations
        decimal totalBookingValue = bookings
            .Where(b => b.Status != "Cancelled")
            .Sum(b => b.TotalAmount);
        decimal totalOutstanding = totalBookingValue - totalRevenue;
        if (totalOutstanding < 0) totalOutstanding = 0;

        // Booking status distribution
        var bookingStatus = new BookingStatusBreakdownDTO
        {
            Pending = bookings.Count(b => b.Status == "Pending"),
            Confirmed = bookings.Count(b => b.Status == "Confirmed"),
            CheckedIn = bookings.Count(b => b.Status == "CheckedIn"),
            CheckedOut = bookings.Count(b => b.Status == "CheckedOut"),
            Cancelled = bookings.Count(b => b.Status == "Cancelled"),
        };

        // Monthly revenue for the last 6 months
        var sixMonthsAgo = today.AddMonths(-5);
        var startOfRange = new DateTime(sixMonthsAgo.Year, sixMonthsAgo.Month, 1);

        var monthlyRevenue = new List<MonthlyRevenueDTO>();
        for (int i = 0; i < 6; i++)
        {
            var monthStart = startOfRange.AddMonths(i);
            var monthEnd = monthStart.AddMonths(1);
            var monthLabel = monthStart.ToString("MMM yyyy");
            var monthAmount = paidPayments
                .Where(p => p.PaymentDate >= monthStart && p.PaymentDate < monthEnd)
                .Sum(p => p.Amount);

            monthlyRevenue.Add(new MonthlyRevenueDTO
            {
                Month = monthLabel,
                Amount = monthAmount,
            });
        }

        // Monthly occupancy for the last 6 months using room-days
        var monthlyOccupancy = new List<MonthlyOccupancyDTO>();
        var nonCancelledBookings = bookings
            .Where(b => b.Status != "Cancelled")
            .ToList();

        for (int i = 0; i < 6; i++)
        {
            var monthStart = startOfRange.AddMonths(i);
            var monthEnd = monthStart.AddMonths(1);
            int daysInMonth = (monthEnd - monthStart).Days;
            int totalRoomDays = totalRooms * daysInMonth;

            if (totalRoomDays == 0)
            {
                monthlyOccupancy.Add(new MonthlyOccupancyDTO
                {
                    Month = monthStart.ToString("MMM yyyy"),
                    Percentage = 0,
                });
                continue;
            }

            int occupiedRoomDays = 0;
            foreach (var booking in nonCancelledBookings)
            {
                var overlapStart = booking.CheckInDate.Date > monthStart
                    ? booking.CheckInDate.Date : monthStart;
                var overlapEnd = booking.CheckOutDate.Date < monthEnd
                    ? booking.CheckOutDate.Date : monthEnd;

                if (overlapEnd > overlapStart)
                {
                    occupiedRoomDays += (overlapEnd - overlapStart).Days;
                }
            }

            double pct = Math.Round((double)occupiedRoomDays / totalRoomDays * 100, 1);
            if (pct > 100) pct = 100;

            monthlyOccupancy.Add(new MonthlyOccupancyDTO
            {
                Month = monthStart.ToString("MMM yyyy"),
                Percentage = pct,
            });
        }

        // Occupancy by room category (current state)
        var roomCategories = rooms
            .GroupBy(r => string.IsNullOrWhiteSpace(r.RoomType) ? "Standard" : r.RoomType)
            .Select(g =>
            {
                int catTotal = g.Count();
                int catOccupied = g.Count(r => occupiedRoomIds.Contains(r.Id));
                double catPct = catTotal > 0
                    ? Math.Round((double)catOccupied / catTotal * 100, 1)
                    : 0;
                return new RoomCategoryOccupancyDTO
                {
                    Category = g.Key,
                    TotalRooms = catTotal,
                    OccupiedRooms = catOccupied,
                    Percentage = catPct,
                };
            })
            .OrderBy(c => c.Category)
            .ToList();

        // Recent 5 bookings
        var recentBookings = bookings
            .OrderByDescending(b => b.BookingDate)
            .Take(5)
            .Select(b =>
            {
                var room = rooms.FirstOrDefault(r => r.Id == b.RoomId);
                var customer = customers.FirstOrDefault(c => c.Id == b.CustomerId);
                return new RecentBookingDTO
                {
                    Id = b.Id,
                    RoomId = b.RoomId,
                    RoomNumber = room?.RoomNumber ?? "",
                    CustomerId = b.CustomerId,
                    CustomerName = customer != null
                        ? $"{customer.FirstName} {customer.LastName}" : "",
                    CheckInDate = b.CheckInDate,
                    CheckOutDate = b.CheckOutDate,
                    Status = b.Status,
                    TotalAmount = b.TotalAmount,
                };
            })
            .ToList();

        // Recent 5 payments
        var recentPayments = payments
            .OrderByDescending(p => p.PaymentDate)
            .Take(5)
            .Select(p => new RecentPaymentDTO
            {
                Id = p.Id,
                BookingId = p.BookingId,
                Amount = p.Amount,
                PaymentMethod = p.PaymentMethod,
                PaymentDate = p.PaymentDate,
                PaymentStatus = p.PaymentStatus,
            })
            .ToList();

        // Payment overview (ledger breakdown)
        var paymentsByBooking = paidPayments
            .GroupBy(p => p.BookingId)
            .ToDictionary(g => g.Key, g => g.Sum(p => p.Amount));

        int fullyPaid = 0, partiallyPaid = 0, unpaid = 0;
        foreach (var booking in nonCancelledBookings)
        {
            decimal paid = paymentsByBooking.GetValueOrDefault(booking.Id, 0m);
            if (paid >= booking.TotalAmount) fullyPaid++;
            else if (paid > 0) partiallyPaid++;
            else unpaid++;
        }

        var paymentOverview = new PaymentOverviewDTO
        {
            TotalPaid = totalRevenue,
            TotalOutstanding = totalOutstanding,
            FullyPaidBookings = fullyPaid,
            PartiallyPaidBookings = partiallyPaid,
            UnpaidBookings = unpaid,
        };

        return new DashboardResponseDTO
        {
            TotalRooms = totalRooms,
            AvailableRooms = availableCount,
            OccupiedRooms = occupiedCount,
            ReservedRooms = reservedCount,
            ActiveBookings = activeBookings,
            TotalCustomers = customers.Count,
            TotalRevenue = totalRevenue,
            OutstandingAmount = totalOutstanding,
            BookingStatus = bookingStatus,
            MonthlyRevenue = monthlyRevenue,
            MonthlyOccupancy = monthlyOccupancy,
            RoomCategoryOccupancy = roomCategories,
            RecentBookings = recentBookings,
            RecentPayments = recentPayments,
            PaymentOverview = paymentOverview,
        };
    }
}
