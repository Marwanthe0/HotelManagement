using HotelManagement.Application.DTOs.Dashboard;

namespace HotelManagement.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardResponseDTO> GetDashboardAsync();
}
