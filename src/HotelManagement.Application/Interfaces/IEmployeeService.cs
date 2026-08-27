using HotelManagement.Application.DTOs.Employees;

namespace HotelManagement.Application.Interfaces;

public interface IEmployeeService
{
    Task<IEnumerable<EmployeeResponseDTO>> GetAllAsync();
    Task<EmployeeResponseDTO?> GetByIdAsync(int id);
    Task<EmployeeResponseDTO> CreateAsync(CreateEmployeeDTO dto);
    Task<EmployeeResponseDTO?> UpdateAsync(int id, UpdateEmployeeDTO dto);
    Task<bool> DeleteAsync(int id);
}
