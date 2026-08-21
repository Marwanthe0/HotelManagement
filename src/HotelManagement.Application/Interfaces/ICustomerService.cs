using HotelManagement.Application.DTOs.Customers;

namespace HotelManagement.Application.Interfaces;

public interface ICustomerService
{
    Task<IEnumerable<CustomerResponseDTO>> GetAllAsync();

    Task<CustomerResponseDTO?> GetByIdAsync(int id);

    Task<CustomerResponseDTO> CreateAsync(CreateCustomerDTO dto);

    Task<CustomerResponseDTO?> UpdateAsync(int id,UpdateCustomerDTO dto);

    Task<bool> DeleteAsync(int id);
}