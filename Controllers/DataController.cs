using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SopraOwaspKata.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DataController : ControllerBase
    {
        private static readonly Dictionary<int, string> Data = new Dictionary<int, string>
        {
            {1, "Data for User 1"},
            {2, "Data for User 2"},
            {3, "Data for User 3"}
        };

        [Authorize(AuthenticationSchemes = "Bearer,LocalJson")]
        [HttpGet("{userId}")]
        public IActionResult GetData(int userId)
        {
            if (!Data.TryGetValue(userId, out string? value))
                return NotFound();

            if (!Data.ContainsKey(userId))
            {
                return NotFound("Data not found.");
            }

            return Ok(value);
        }

        [Authorize(AuthenticationSchemes = "Bearer,LocalJson")]
        [HttpGet]
        public IActionResult SecureEndpoint()
        {
            return Ok("You are authorized!");
        }
    }
}
