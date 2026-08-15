using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.API.Controllers;


[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{

    [HttpGet]
    public string Get()
    {
        return "Hotel Management API is working!";
    }

}