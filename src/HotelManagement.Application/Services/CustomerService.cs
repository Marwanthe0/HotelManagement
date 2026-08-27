using HotelManagement.Application.DTOs.Customers;
using HotelManagement.Application.Interfaces;
using HotelManagement.Domain.Entities;

namespace HotelManagement.Application.Services;

public class CustomerService:ICustomerService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IBookingRepository _bookingRepository;

    public CustomerService(
        ICustomerRepository customerRepository,
        IBookingRepository bookingRepository)
    {
        _customerRepository = customerRepository;
        _bookingRepository = bookingRepository;
    }

    public async Task<IEnumerable<CustomerResponseDTO>> GetAllAsync(string? search = null)
    {
        var customers = string.IsNullOrWhiteSpace(search)
            ? await _customerRepository.GetAllAsync()
            : await _customerRepository.SearchAsync(search.Trim());

        return customers.Select(c => MapToResponseDTO(c));
    }


    public async Task<CustomerResponseDTO?> GetByIdAsync(int id)
    {
        var customer = await _customerRepository.GetByIdAsync(id);

        if(customer is null) return null;

        return MapToResponseDTO(customer);
    }

    public async Task<CustomerResponseDTO> CreateAsync(CreateCustomerDTO dto)
    {
        var emailExists = await _customerRepository.ExistsByEmailAsync(dto.Email);
        if(emailExists)
        {
            throw new InvalidOperationException("A customer with this email already exists.");
        }
        var customer = new Customer 
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Phone = dto.Phone,
            Address = dto.Address
        };
        
        await _customerRepository.AddAsync(customer);

        return MapToResponseDTO(customer);
    }

    public async Task<CustomerResponseDTO?> UpdateAsync(int id,UpdateCustomerDTO dto)
    {
        var customer = await _customerRepository.GetByIdAsync(id);
        if(customer is null) return null;

        var emailExists = await _customerRepository.ExistsByEmailAsync(dto.Email, id);
        if(emailExists)
        {
            throw new InvalidOperationException("A customer with this email already exists.");
        }

        customer.FirstName = dto.FirstName;
        customer.LastName = dto.LastName;
        customer.Email = dto.Email;
        customer.Phone = dto.Phone;
        customer.Address = dto.Address;
        await _customerRepository.UpdateAsync(customer);

        return MapToResponseDTO(customer);
    }
    public async Task<bool> DeleteAsync(int id)
    {
        var customer = await _customerRepository.GetByIdAsync(id);
        if(customer is null) return false;

        // Bookings use DeleteBehavior.Restrict, so deleting a customer that still has
        // booking history would fail at the database level. Reject it with a clear
        // business error instead of surfacing a raw database exception.
        var hasBookings = await _bookingRepository.HasBookingsForCustomerAsync(id);
        if (hasBookings)
        {
            throw new InvalidOperationException(
                "Cannot delete customer because they have existing bookings.");
        }

        await _customerRepository.DeleteAsync(customer);
        return true;
    }

    private static CustomerResponseDTO MapToResponseDTO(Customer customer)
    {
        return new CustomerResponseDTO
        {
            Id = customer.Id,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            Email = customer.Email,
            Phone = customer.Phone,
            Address = customer.Address
        };
    }
}


