namespace HotelManagement.Application.DTOs.Dashboard;

public class DashboardResponseDTO
{
    // Summary metrics
    public int TotalRooms { get; set; }
    public int AvailableRooms { get; set; }
    public int OccupiedRooms { get; set; }
    public int ReservedRooms { get; set; }
    public int ActiveBookings { get; set; }
    public int TotalCustomers { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal OutstandingAmount { get; set; }

    // Booking status breakdown
    public BookingStatusBreakdownDTO BookingStatus { get; set; } = new();

    // Monthly analytics
    public List<MonthlyRevenueDTO> MonthlyRevenue { get; set; } = new();
    public List<MonthlyOccupancyDTO> MonthlyOccupancy { get; set; } = new();

    // Room category occupancy
    public List<RoomCategoryOccupancyDTO> RoomCategoryOccupancy { get; set; } = new();

    // Recent activity
    public List<RecentBookingDTO> RecentBookings { get; set; } = new();
    public List<RecentPaymentDTO> RecentPayments { get; set; } = new();

    // Payment overview
    public PaymentOverviewDTO PaymentOverview { get; set; } = new();
}

public class BookingStatusBreakdownDTO
{
    public int Pending { get; set; }
    public int Confirmed { get; set; }
    public int CheckedIn { get; set; }
    public int CheckedOut { get; set; }
    public int Cancelled { get; set; }
}

public class MonthlyRevenueDTO
{
    public string Month { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class MonthlyOccupancyDTO
{
    public string Month { get; set; } = string.Empty;
    public double Percentage { get; set; }
}

public class RoomCategoryOccupancyDTO
{
    public string Category { get; set; } = string.Empty;
    public int TotalRooms { get; set; }
    public int OccupiedRooms { get; set; }
    public double Percentage { get; set; }
}

public class RecentBookingDTO
{
    public int Id { get; set; }
    public int RoomId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
}

public class RecentPaymentDTO
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public DateTime PaymentDate { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
}

public class PaymentOverviewDTO
{
    public decimal TotalPaid { get; set; }
    public decimal TotalOutstanding { get; set; }
    public int FullyPaidBookings { get; set; }
    public int PartiallyPaidBookings { get; set; }
    public int UnpaidBookings { get; set; }
}
