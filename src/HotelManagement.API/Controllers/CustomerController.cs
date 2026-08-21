using HotelManagement.Application.DTOs.Customers;
using HotelManagement.Application.Interfaces;
using HotelManagement.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.API.Controllers;

[ApiController]
[Route("api/controller")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerSerivce;

    public CustomersController(ICustomerService customerService)
    {
        _customerSerivce = customerService;
    }

    //GET /api/customers
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CustomerResponseDTO>>> GetAll()
    {
        var customers = await _customerSerivce.GetAllAsync();
        return Ok(customers);
    }

    //GET /api/customers/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<CustomerResponseDTO>> GetById(int id)
    {
        var customer = await _customerSerivce.GetByIdAsync(id);
        if (customer is null)
        {
            return NotFound();
        }
        return Ok(customer);
    }

    //POST /api/controller
    [HttpPost]
    public async Task<ActionResult<CustomerResponseDTO>> Create(CreateCustomerDTO dto)
    {
        var customer = await _customerSerivce.CreateAsync(dto);

        return CreatedAtAction(
            nameof(GetById),
            new { id = customer.Id },
            customer
        );
    }


    //PUT /api/customers/{id}
    [HttpPut("{id:int}")]
    public async Task<ActionResult<CustomerResponseDTO>> Update(int id, UpdateCustomerDTO dto)
    {
        var customer = await _customerSerivce.UpdateAsync(id, dto);

        if (customer is null) return NotFound();
        return Ok(customer);
    }

    //DELETE /api/customers/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _customerSerivce.DeleteAsync(id);

        if (!deleted) return NotFound();
        return NoContent();
    }
    
    
}