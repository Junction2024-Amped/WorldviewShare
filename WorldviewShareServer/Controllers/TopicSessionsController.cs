using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorldviewShareServer.Data;
using WorldviewShareServer.Dtos;
using WorldviewShareServer.Models;

namespace WorldviewShareServer.Controllers
{
    [Route("api/topics")]
    [ApiController]
    public class TopicSessionsController : ControllerBase
    {
        private readonly WorldviewShareContext _context;

        public TopicSessionsController(WorldviewShareContext context)
        {
            _context = context;
        }

        private TopicSession ToTopicSession(TopicSessionRequestDto topicSessionRequestDto)
        {
            var topicSession = new TopicSession
            {
                Name = topicSessionRequestDto.Name,
                Topic = topicSessionRequestDto.Topic
            };
            _context.TopicSessions.Add(topicSession);
            return topicSession;
        }
        
        private async Task<TopicSession?> GetTopicSessionById(Guid id) => await _context.TopicSessions.FindAsync(id);
        
        private static TopicSessionResponseDto ToTopicSessionResponseDto(TopicSession topicSession) => new(topicSession.Id, topicSession.Name, topicSession.Topic, topicSession.Users.Select(u => u.Id).ToList());

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TopicSessionResponseDto>>> GetTopicSessions()
        {
            return await _context.TopicSessions.Select(ts => ToTopicSessionResponseDto(ts)).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TopicSessionResponseDto>> GetTopicSession(Guid id)
        {
            var topicSession = await GetTopicSessionById(id);

            if (topicSession == null)
            {
                return NotFound();
            }

            return ToTopicSessionResponseDto(topicSession);
        }

        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTopicSession(Guid id, TopicSessionRequestDto topicSessionDto)
        {
            var topicSession = await GetTopicSessionById(id);
            if (topicSession == null)
            {
                return NotFound();
            }
            topicSession.Name = topicSessionDto.Name;
            topicSession.Topic = topicSessionDto.Topic;
            _context.Entry(topicSession).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TopicSessionExists(id))
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
            var topicSession = ToTopicSession(topicSessionDto);
            _context.TopicSessions.Add(topicSession);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTopicSession", new { id = topicSession.Id }, ToTopicSessionResponseDto(topicSession));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTopicSession(Guid id)
        {
            var topicSession = await GetTopicSessionById(id);
            if (topicSession == null)
            {
                return NotFound();
            }

            _context.TopicSessions.Remove(topicSession);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TopicSessionExists(Guid id)
        {
            return _context.TopicSessions.Any(e => e.Id == id);
        }
    }
}
