using HotelManagement.Application.DTOs.Customers;
using HotelManagement.Application.Interfaces;
using HotelManagement.Domain.Entities;

namespace HotelManagement.Application.Services;

public class CustomerService:ICustomerService
{
    private readonly ICustomerRepository _customerRepository;
    
    public CustomerService(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<IEnumerable<CustomerResponseDTO>> GetAllAsync()
    {
        var customers = await _customerRepository.GetAllAsync();

        return customers.select(c=>new CustomerResponseDTO
        {
            Id = c.Id,
            FirstName = c.FirstName,
            LastName = c.LastName,
            Email = c.Email,
            Phone = c.Phone,
            Address = c.Address
        })
    }
}