using Microsoft.AspNetCore.Mvc;
using WaterLevelAPI.Service;

namespace WaterLevelAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WaterLevelController : ControllerBase
    {
        private readonly ILogger<WaterLevelController> _logger;
        private readonly IWaterLevelService _service;

        public WaterLevelController(ILogger<WaterLevelController> logger, IWaterLevelService service)
        {
            _logger = logger;
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> RegisterLevel([FromBody] WaterLevelDTO waterLevelDTO)
        {
            try
            {
                await _service.RegisterLevelAsync(waterLevelDTO);
                return Accepted();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = ex.Message });
            }
        }

        [HttpGet("current")]
        public async Task<IActionResult> GetCurrentLevel(string deviceId)
        {
            var result = await _service.GetLevelAsync(deviceId);
            return Accepted(result);
        }

        [HttpGet("status-device")]
        public async Task<IActionResult> GetStatusDevice(string deviceId)
        {
            var result = await _service.GetStatusDevice(deviceId);

            return Accepted(result);
        }

        [HttpPost("status-device")]
        public async Task<IActionResult> SetStatusDevice([FromBody] PendingChangesDTO changesDTO)
        {
            try
            {
                await _service.SetStatusDevice(changesDTO);
                return Accepted();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = ex.Message });
            }
        }
    }
}
