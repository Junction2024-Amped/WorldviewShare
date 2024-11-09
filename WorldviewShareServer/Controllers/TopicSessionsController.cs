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
            return (await _topicSessionsService.GetTopicSessions()).Select(ts => _topicSessionsService.ToTopicSessionResponseDto(ts)).ToList();
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
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTopicSession(Guid id, TopicSessionRequestDto topicSessionDto)
        {
            var topicSession = await _topicSessionsService.GetTopicSessionById(id);
            if (topicSession == null)
            {
                return NotFound();
            }
            topicSession.Name = topicSessionDto.Name;
            topicSession.Topic = topicSessionDto.Topic;
            _topicSessionsService.SetEntityState(topicSession, EntityState.Modified);

            try
            {
                await _topicSessionsService.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_topicSessionsService.TopicSessionExists(id))
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
        public async Task<ActionResult<TopicSessionResponseDto>> PostTopicSession(TopicSessionRequestDto topicSessionDto)
        {
            var topicSession = _topicSessionsService.ToTopicSession(topicSessionDto);
            await _topicSessionsService.SaveChangesAsync();

            return CreatedAtAction("GetTopicSession", new { id = topicSession.Id }, _topicSessionsService.ToTopicSessionResponseDto(topicSession));
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

            _topicSessionsService.RemoveTopicSession(topicSession);
            await _topicSessionsService.SaveChangesAsync();

            return NoContent();
        }
    }
}
