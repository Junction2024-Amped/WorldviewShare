using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorldviewShareServer.Data;
using WorldviewShareServer.Dtos;
using WorldviewShareServer.Models;

namespace WorldviewShareServer.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly WorldviewShareContext _context;

        public UsersController(WorldviewShareContext context)
        {
            _context = context;
        }
        
        private User ToUser(UserRequestDto userRequestDto)
        {
            var user = new User
            {
                Username = userRequestDto.Username
            };
            _context.Users.Add(user);
            return user;
        }
        
        private async Task<User?> GetUserById(Guid id) => await _context.Users.FindAsync(id);
        
        private static UserResponseDto ToUserResponseDto(User user) => new(user.Id, user.Username, user.TopicSessions.Select(ts => ts.Id).ToList());

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetUsers()
        {
            return await _context.Users.Select(u => ToUserResponseDto(u)).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserResponseDto>> GetUser(Guid id)
        {
            var user = await GetUserById(id);

            if (user == null)
            {
                return NotFound();
            }

            return ToUserResponseDto(user);
        }

        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(Guid id, UserRequestDto userDto)
        {
            var user = await GetUserById(id);
            if (user == null)
            {
                return NotFound();
            }
            user.Username = userDto.Username;
            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
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
            var user = ToUser(userDto);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUser", new { id = user.Id }, ToUserResponseDto(user));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var user = await GetUserById(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserExists(Guid id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
