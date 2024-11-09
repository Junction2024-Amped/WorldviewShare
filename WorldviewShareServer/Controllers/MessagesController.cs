using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorldviewShareServer.Data;
using WorldviewShareServer.Dtos;
using WorldviewShareServer.Models;

namespace WorldviewShareServer.Controllers
{
    [Route("api/messages")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly WorldviewShareContext _context;

        public MessagesController(WorldviewShareContext context)
        {
            _context = context;
        }
        
        private Message ToMessage(MessageRequestDto messageDto)
        {
            var message = new Message
            {
                Content = messageDto.Content,
                TopicSessionId = messageDto.TopicSessionId,
                AuthorId = messageDto.AuthorId,
                CreatedAt = DateTime.Now
            };
            _context.Messages.Add(message);
            return message;
        }
        
        private async Task<Message?> GetMessageById(Guid id) => await _context.Messages.FindAsync(id);
        
        private static MessageResponseDto ToMessageResponseDto(Message message) => new(message.Id, message.Content, message.TopicSessionId, message.AuthorId, message.CreatedAt);

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MessageResponseDto>>> GetMessages()
        {
            return await _context.Messages.Select(m => ToMessageResponseDto(m)).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MessageResponseDto>> GetMessage(Guid id)
        {
            var message = await GetMessageById(id);

            if (message == null)
            {
                return NotFound();
            }

            return ToMessageResponseDto(message);
        }

        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMessage(Guid id, MessageRequestDto messageDto)
        {
            var message = await GetMessageById(id);
            if (message == null)
            {
                return NotFound();
            }
            
            message.Content = messageDto.Content;
            message.TopicSessionId = messageDto.TopicSessionId;
            message.AuthorId = messageDto.AuthorId;
            _context.Entry(message).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MessageExists(id))
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
        public async Task<ActionResult<MessageResponseDto>> PostMessage(MessageRequestDto messageDto)
        {
            var message = ToMessage(messageDto);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetMessage", new { id = message.Id }, ToMessageResponseDto(message));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMessage(Guid id)
        {
            var message = await GetMessageById(id);
            if (message == null)
            {
                return NotFound();
            }

            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool MessageExists(Guid id)
        {
            return _context.Messages.Any(e => e.Id == id);
        }
    }
}
