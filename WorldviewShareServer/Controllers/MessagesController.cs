using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorldviewShareServer.Services;
using WorldviewShareShared.DTO.Request.Messages;
using WorldviewShareShared.DTO.Response.Messages;

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
        [HttpPost]
        public async Task<ActionResult<MessageResponseDto>> PostMessage(MessageRequestDto messageDto)
        {
            var message = _service.ToMessage(messageDto);
            await _service.SaveChangesAsync();

            return CreatedAtAction("GetMessage", new { id = message.Id }, _service.ToMessageResponseDto(message));
        }
    }
}
