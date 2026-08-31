using HotelManagement.Application.DTOs.Bookings;
using HotelManagement.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.API.Controllers;

[Authorize]
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
    // GET /api/bookings?status=Confirmed
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BookingResponseDTO>>> GetAll(
        [FromQuery] string? status)
    {
        var bookings = await _bookingService.GetAllAsync(status);
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

        return CreatedAtAction(nameof(GetById), new { id = booking.Id }, booking);
    }

    //PUT /api/bookings/{id}
    [HttpPut("{id:int}")]
    public async Task<ActionResult<BookingResponseDTO>> Update(int id, UpdateBookingDTO dto)
    {
        var booking = await _bookingService.UpdateAsync(id, dto);
        if (booking is null)
            return NotFound();

        return Ok(booking);
    }

    //PATCH /api/bookings/{id}/cancel
    [HttpPatch("{id:int}/cancel")]
    public async Task<ActionResult<BookingResponseDTO>> Cancel(int id)
    {
        var booking = await _bookingService.CancelAsync(id);
        if (booking is null)
            return NotFound();
        return Ok(booking);
    }

    //PATCH /api/bookings/{id}/check-in
    [HttpPatch("{id:int}/check-in")]
    public async Task<ActionResult<BookingResponseDTO>> CheckIn(int id)
    {
        var booking = await _bookingService.CheckInAsync(id);
        if (booking is null)
            return NotFound();
        return Ok(booking);
    }

    //PATCH /api/bookings/{id}/check-out
    [HttpPatch("{id:int}/check-out")]
    public async Task<ActionResult<BookingResponseDTO>> CheckOut(int id)
    {
        var booking = await _bookingService.CheckOutAsync(id);
        if (booking is null)
            return NotFound();
        return Ok(booking);
    }

    //DELETE /api/bookings/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _bookingService.DeleteAsync(id);
        if (!deleted)
            return NotFound();
        return NoContent();
    }
}
