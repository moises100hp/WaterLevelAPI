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

        [HttpPost(Name = "telemetry")]
        public async Task<IActionResult> RegisterLevel(WaterLevelDTO waterLevelDTO)
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

        [HttpGet(Name = "current")]
        public async Task<IActionResult> GetCurrentLevel(int deviceId)
        {
            var result = await _service.GetLevelAsync(deviceId);
            return Accepted(result);
        }

    }
}
