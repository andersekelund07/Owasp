using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SopraOwaspKata.Dto;
using SopraOwaspKata.Repository;
using System.Security.Claims;

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
            string? currentUserName = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var currentUser = _userRepository.GetUserByUserName(currentUserName);
            string? userRole = HttpContext.User.FindFirst(ClaimTypes.Role)?.Value;
            if (currentUser.Id != id && userRole != "Editor")
            {
                return StatusCode(403); // Forbidden
            }

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

        [HttpPost("login")]
        public IActionResult Login([FromBody] UserLoginDto userLogin)
        {
            if (_userRepository.IsAccountLockedOut(userLogin.Username))
            {
                return StatusCode(429, "Account is temporarily locked due to too many failed login attempts.");
            }
            var user = _userRepository.AuthenticateUser(userLogin.Username, userLogin.Password);
            if (user.IsAuthenticated)
            {
                // This would ideally return a secure token or session ID.
                return Ok(user);
            }
            else
            {
                return Unauthorized(user.Message);
            }
        }

        [Authorize(AuthenticationSchemes = "Bearer,LocalJson")]
        [HttpPost]
        public IActionResult CreateUser([FromBody] CreateUserDto createUserDto)
        {
            var success = _userRepository.CreateUser(createUserDto);
            if (success)
                return Ok();
            else
                return BadRequest();
        }
    }
}
