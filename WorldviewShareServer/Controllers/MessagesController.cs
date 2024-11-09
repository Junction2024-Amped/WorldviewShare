using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorldviewShareServer.Dtos;
using WorldviewShareServer.Services;

namespace WorldviewShareServer.Controllers
{
    [Route("api/messages")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly MessagesService _service;

        public MessagesController(MessagesService service)
        {
            _service = service;
        }
        

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MessageResponseDto>>> GetMessages()
        {
            return (await _service.GetMessages()).Select(_service.ToMessageResponseDto).ToList();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MessageResponseDto>> GetMessage(Guid id)
        {
            var message = await _service.GetMessageById(id);

            if (message == null)
            {
                return NotFound();
            }

            return _service.ToMessageResponseDto(message);
        }

        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMessage(Guid id, MessageRequestDto messageDto)
        {
            var message = await _service.GetMessageById(id);
            if (message == null)
            {
                return NotFound();
            }
            
            message.Content = messageDto.Content;
            message.TopicSessionId = messageDto.TopicSessionId;
            message.AuthorId = messageDto.AuthorId;
            _service.SetSEntityState(message, EntityState.Modified);

            try
            {
                await _service.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_service.MessageExists(id))
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
            var message = _service.ToMessage(messageDto);
            await _service.SaveChangesAsync();

            return CreatedAtAction("GetMessage", new { id = message.Id }, _service.ToMessageResponseDto(message));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMessage(Guid id)
        {
            var message = await _service.GetMessageById(id);
            if (message == null)
            {
                return NotFound();
            }
            
            _service.RemoveMessage(message);
            await _service.SaveChangesAsync();

            return NoContent();
        }
    }
}
