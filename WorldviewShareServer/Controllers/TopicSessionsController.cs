using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorldviewShareServer.Services;
using WorldviewShareShared.DTO.Request.TopicSessions;
using WorldviewShareShared.DTO.Request.Users;
using WorldviewShareShared.DTO.Response.Messages;
using WorldviewShareShared.DTO.Response.TopicSessions;

namespace WorldviewShareServer.Controllers
{
    [Route("api/topics")]
    [ApiController]
    public class TopicSessionsController : ControllerBase
    {
        private readonly TopicSessionsService _topicSessionsService;
        private readonly UsersService _usersService;
        private readonly MessagesService _messagesService;

        public TopicSessionsController(TopicSessionsService topicSessionsService, UsersService usersService, MessagesService messagesService)
        {
            _topicSessionsService = topicSessionsService;
            _usersService = usersService;
            _messagesService = messagesService;
        }
        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TopicSessionResponseDto>>> GetTopicSessions()
        {
            return (await _topicSessionsService.GetTopicSessions()).Where(ts => !ts.Archived).Select(ts => _topicSessionsService.ToTopicSessionResponseDto(ts)).ToList();
        }
        
        [HttpGet("random")]
        public async Task<ActionResult<TopicSessionResponseDto>> GetRandomTopicSession()
        {
            var topicSessions = (await _topicSessionsService.GetTopicSessions()).Where(ts => !ts.Archived).ToList();
            var totalUsers = topicSessions.Sum(ts => ts.Users.Count);
            var random = new Random().Next(totalUsers);
            var sum = 0;
            foreach (var topicSession in topicSessions)
            {
                var invertedCount = Math.Abs(totalUsers - sum);
                sum += invertedCount;
                if (sum >= random)
                {
                    return _topicSessionsService.ToTopicSessionResponseDto(topicSession);
                }
            }
            if (topicSessions.Count > 0)
            {
                return _topicSessionsService.ToTopicSessionResponseDto(topicSessions.Last());
            }
            return NotFound();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TopicSessionResponseDto>> GetTopicSession(Guid id)
        {
            var topicSession = await _topicSessionsService.GetTopicSessionById(id);

            if (topicSession == null)
            {
                return NotFound();
            }

            return _topicSessionsService.ToTopicSessionResponseDto(topicSession);
        }
        
        [HttpGet("{id}/messages")]
        public async Task<ActionResult<IEnumerable<MessageResponseDto>>> GetTopicSessionMessages(Guid id)
        {
            var topicSession = await _topicSessionsService.GetTopicSessionById(id);
            if (topicSession == null)
            {
                return NotFound();
            }
            return (await _messagesService.GetMessages()).Where(m => m.TopicSession == topicSession).Select(_messagesService.ToMessageResponseDto).ToList();
        }

        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<TopicSessionResponseDto>> PostTopicSession(TopicSessionRequestDto topicSessionDto)
        {
            var topicSession = _topicSessionsService.ToTopicSession(topicSessionDto);
            await _topicSessionsService.SaveChangesAsync();

            return CreatedAtAction("GetTopicSession", new { id = topicSession.Id }, _topicSessionsService.ToTopicSessionResponseDto(topicSession));
        }
        
        [HttpPost("{id}/status")]
        public async Task<IActionResult> SetTopicSessionStatus(Guid id, TopicSessionStatusRequestDto statusDto)
        {
            var topicSession = await _topicSessionsService.GetTopicSessionById(id);
            if (topicSession == null)
            {
                return NotFound();
            }
            topicSession.Archived = statusDto.Archived;
            _topicSessionsService.SetEntityState(topicSession, EntityState.Modified);
            await _topicSessionsService.SaveChangesAsync();
            return NoContent();
        }
        
        [HttpPatch("{id}")]
        public async Task<IActionResult> ToggleTopicParticipant(Guid id, UserReferenceRequestDto userDto)
        {
            var topicSession = await _topicSessionsService.GetTopicSessionById(id);
            if (topicSession == null)
            {
                return NotFound();
            }
            var user = await _usersService.GetUserById(userDto.Id);
            if (user == null)
            {
                return NotFound();
            }
            if (topicSession.Users.Contains(user))
            {
                topicSession.Users.Remove(user);
            }
            else
            {
                topicSession.Users.Add(user);
            }
            _topicSessionsService.SetEntityState(topicSession, EntityState.Modified);
            await _topicSessionsService.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTopicSession(Guid id)
        {
            var topicSession = await _topicSessionsService.GetTopicSessionById(id);
            if (topicSession == null)
            {
                return NotFound();
            }

            topicSession.Archived = true;
            _topicSessionsService.SetEntityState(topicSession, EntityState.Modified);
            await _topicSessionsService.SaveChangesAsync();

            return NoContent();
        }
    }
}
