using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SopraOwaspKata.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;

        public UsersController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        /// <summary>
        /// User get his/hers own information
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(AuthenticationSchemes = "Bearer,LocalJson")]
        [HttpGet("{id}")]
        public IActionResult GetUser(int id)
        {
            var user = _userRepository.GetUserById(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        [Authorize(AuthenticationSchemes = "Bearer,LocalJson")]
        [HttpPut("{id}/role")]
        public IActionResult ChangeUserRole(int id, [FromBody] string newRole)
        {
            string? userRole = HttpContext.User.FindFirst(ClaimTypes.Role)?.Value;
            if (userRole != "Admin")
            {
                return StatusCode(403); // Forbidden
            }

            if (_userRepository.UpdateUserRole(id, newRole))
            {
                return Ok();
            }
            return Ok();
        }

        [Authorize(AuthenticationSchemes = "Bearer,LocalJson")]
        [HttpDelete("{id}")]
        public IActionResult DeleteUser(int id)
        {
            var user = _userRepository.GetUserById(id);
            if (user == null) return NotFound();
            // No role check implemented
            return Ok();
        }
    }
}
