using HotelManagement.Application.DTOs.Bookings;
using HotelManagement.Application.DTOs.Customers;
using HotelManagement.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;
    private readonly IBookingService _bookingService;

    public CustomersController(ICustomerService customerService, IBookingService bookingService)
    {
        _customerService = customerService;
        _bookingService = bookingService;
    }

    //GET /api/customers
    //GET /api/customers?search=John
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CustomerResponseDTO>>> GetAll(
        [FromQuery] string? search)
    {
        var customers = await _customerService.GetAllAsync(search);
        return Ok(customers);
    }

    //GET /api/customers/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<CustomerResponseDTO>> GetById(int id)
    {
        var customer = await _customerService.GetByIdAsync(id);
        if (customer is null)
            return NotFound();

        return Ok(customer);
    }

    //POST /api/customers
    [HttpPost]
    public async Task<ActionResult<CustomerResponseDTO>> Create(CreateCustomerDTO dto)
    {
        var customer = await _customerService.CreateAsync(dto);

        return CreatedAtAction(nameof(GetById), new { id = customer.Id }, customer);
    }

    //PUT /api/customers/{id}
    [HttpPut("{id:int}")]
    public async Task<ActionResult<CustomerResponseDTO>> Update(int id, UpdateCustomerDTO dto)
    {
        var customer = await _customerService.UpdateAsync(id, dto);

        if (customer is null)
            return NotFound();
        return Ok(customer);
    }

    //DELETE /api/customers/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _customerService.DeleteAsync(id);

        if (!deleted)
            return NotFound();
        return NoContent();
    }

    //GET /api/customers/{customerId}/bookings
    [HttpGet("{customerId:int}/bookings")]
    public async Task<ActionResult<IEnumerable<BookingResponseDTO>>> GetBookings(int customerId)
    {
        // First verify the customer exists
        var customer = await _customerService.GetByIdAsync(customerId);
        if (customer is null)
            return NotFound();

        var bookings = await _bookingService.GetByCustomerIdAsync(customerId);
        return Ok(bookings);
    }
}
