using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorldviewShareServer.Data;
using WorldviewShareServer.Dtos;
using WorldviewShareServer.Models;
using WorldviewShareServer.Services;

namespace WorldviewShareServer.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UsersService _service;

        public UsersController(UsersService service)
        {
            _service = service;
        }
        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetUsers()
        {
            return (await _service.GetUsers()).Select(u => _service.ToUserResponseDto(u)).ToList();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserResponseDto>> GetUser(Guid id)
        {
            var user = await _service.GetUserById(id);

            if (user == null)
            {
                return NotFound();
            }

            return _service.ToUserResponseDto(user);
        }

        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(Guid id, UserRequestDto userDto)
        {
            var user = await _service.GetUserById(id);
            if (user == null)
            {
                return NotFound();
            }
            user.Username = userDto.Username;
            _service.SetEntityState(user, EntityState.Modified);

            try
            {
                await _service.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_service.UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<UserResponseDto>> PostUser(UserRequestDto userDto)
        {
            var user =_service.ToUser(userDto);
            await _service.SaveChangesAsync();

            return CreatedAtAction("GetUser", new { id = user.Id }, _service.ToUserResponseDto(user));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var user = await _service.GetUserById(id);
            if (user == null)
            {
                return NotFound();
            }

            _service.RemoveUser(user);
            await _service.SaveChangesAsync();

            return NoContent();
        }
    }
}
