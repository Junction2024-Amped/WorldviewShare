using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorldviewShareServer.Data;
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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TopicSession>>> GetTopicSessions()
        {
            return await _context.TopicSessions.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TopicSession>> GetTopicSession(Guid id)
        {
            var topicSession = await _context.TopicSessions.FindAsync(id);

            if (topicSession == null)
            {
                return NotFound();
            }

            return topicSession;
        }

        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTopicSession(Guid id, TopicSession topicSession)
        {
            if (id != topicSession.Id)
            {
                return BadRequest();
            }

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
        public async Task<ActionResult<TopicSession>> PostTopicSession(TopicSession topicSession)
        {
            _context.TopicSessions.Add(topicSession);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTopicSession", new { id = topicSession.Id }, topicSession);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTopicSession(Guid id)
        {
            var topicSession = await _context.TopicSessions.FindAsync(id);
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
