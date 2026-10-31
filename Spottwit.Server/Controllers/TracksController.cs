using Microsoft.AspNetCore.Mvc;
using Spottwit.Server.Models.Entities;
using Spottwit.Server.Services;

namespace Spottwit.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TracksController : ControllerBase
    {
        private readonly IGenericService<Track> _trackService;

        public TracksController(IGenericService<Track> trackService)
        {
            _trackService = trackService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _trackService.GetAllAsync());

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var track = await _trackService.GetByIdAsync(id);
            return track == null ? NotFound() : Ok(track);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Track track)
        {
            var created = await _trackService.CreateAsync(track);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _trackService.DeleteAsync(id);
            return Ok(new { message = "Успешно удалено" });
        }
    }
}