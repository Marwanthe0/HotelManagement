using HotelManagement.Application.DTOs.Bookings;
using HotelManagement.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    // GET /api/bookings
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BookingResponseDTO>>> GetAll()
    {
        var bookings = await _bookingService.GetAllAsync();
        return Ok(bookings);
    }

    // GET /api/bookings/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<BookingResponseDTO>> GetById(int id)
    {
        var booking = await _bookingService.GetByIdAsync(id);

        if (booking is null)
            return NotFound();
        return Ok(booking);
    }

    // POST /api/bookings
    [HttpPost]
    public async Task<ActionResult<BookingResponseDTO>> Create(CreateBookingDTO dto)
    {
        var booking = await _bookingService.CreateAsync(dto);

        return CreatedAtAction(nameof(GetById),
                               new { id = booking.Id }, booking);
    }

    //PUT /api/bookings/{id}
    [HttpPut("{id:int}")]
    public async Task<ActionResult<BookingResponseDTO>> Update(int id, UpdateBookingDTO dto)
    {
        var booking = await _bookingService.UpdateAsync(id, dto);
        if (booking is null) return NotFound();

        return Ok(booking);
    }

    //DELETE /api/bookings/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _bookingService.DeleteAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }
}