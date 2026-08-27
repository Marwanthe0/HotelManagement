using HotelManagement.Application.DTOs.Rooms;
using HotelManagement.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class RoomsController : ControllerBase
{
    private readonly IRoomService _roomService;

    public RoomsController(IRoomService roomService)
    {
        _roomService = roomService;
    }

    //GET: api/rooms
    [HttpGet]
    public async Task<ActionResult<IEnumerable<RoomResponseDto>>> GetAllRooms()
    {
        var rooms = await _roomService.GetAllRoomsAsync();
        return Ok(rooms);
    }

    //GET: api/rooms/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<RoomResponseDto>> GetRoomById(int id)
    {
        var room = await _roomService.GetRoomByIdAsync(id);

        if (room is null)
            return NotFound();

        return Ok(room);
    }

    //POST: api/rooms
    [HttpPost]
    public async Task<ActionResult<RoomResponseDto>> CreateRoom(CreateRoomDTO dto)
    {
        var room = await _roomService.AddRoomAsync(dto);
        return CreatedAtAction(nameof(GetRoomById), new { id = room.Id }, room);
    }

    //PUT: api/rooms/{id}
    [HttpPut("{id:int}")]
    public async Task<ActionResult<RoomResponseDto>> UpdateRoom(int id, UpdateRoomDto dto)
    {
        var room = await _roomService.UpdateRoomAsync(id, dto);
        if (room is null)
            return NotFound();

        return Ok(room);
    }

    //DELETE: api/rooms/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteRoom(int id)
    {
        var deleted = await _roomService.DeleteRoomAsync(id);
        if (!deleted)
            return NotFound();

        return NoContent();
    }

    //GET: api/rooms/available?checkInDate=...&checkOutDate=...
    [HttpGet("available")]
    public async Task<ActionResult<IEnumerable<RoomResponseDto>>> GetAvailableRooms(
        [FromQuery] DateTime checkInDate,
        [FromQuery] DateTime checkOutDate)
    {
        var rooms = await _roomService.GetAvailableRoomsAsync(checkInDate, checkOutDate);
        return Ok(rooms);
    }
}