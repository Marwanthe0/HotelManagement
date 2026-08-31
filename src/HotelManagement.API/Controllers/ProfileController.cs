using System.Security.Claims;
using HotelManagement.Application.DTOs.Profile;
using HotelManagement.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ProfileController : ControllerBase
{
    private readonly IProfileService _profileService;

    public ProfileController(IProfileService profileService)
    {
        _profileService = profileService;
    }

    // GET: api/profile
    [HttpGet]
    public async Task<ActionResult<UserProfileDTO>> GetProfile()
    {
        var userId = GetUserId();
        var profile = await _profileService.GetProfileAsync(userId);
        return Ok(profile);
    }

    // PUT: api/profile
    [HttpPut]
    public async Task<ActionResult<UserProfileDTO>> UpdateProfile([FromBody] UpdateProfileDTO dto)
    {
        var userId = GetUserId();
        var updated = await _profileService.UpdateProfileAsync(userId, dto);
        return Ok(updated);
    }

    // PUT: api/profile/change-password
    [HttpPut("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO dto)
    {
        var userId = GetUserId();
        await _profileService.ChangePasswordAsync(userId, dto);
        return Ok(new { message = "Password updated successfully." });
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User identity claim missing.");
        }
        return userId;
    }
}
