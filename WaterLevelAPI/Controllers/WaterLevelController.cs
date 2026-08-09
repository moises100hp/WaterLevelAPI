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

        // POST api/waterlevel
        [HttpPost]
        public async Task<IActionResult> RegisterLevel(WaterLevelDTO waterLevelDTO)
        {
            try
            {
                await _service.RegisterLevelAsync(waterLevelDTO);
                return Accepted();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao registrar leitura do dispositivo {DeviceId}", waterLevelDTO.DeviceId);
                return StatusCode(500, new { Message = "Erro interno ao registrar a leitura." });
            }
        }

        // GET api/waterlevel?deviceId=esp32-01
        [HttpGet]
        public async Task<IActionResult> GetCurrentLevel([FromQuery] string deviceId)
        {
            if (string.IsNullOrWhiteSpace(deviceId))
                return BadRequest(new { Message = "Informe o deviceId." });

            var result = await _service.GetLevelAsync(deviceId);

            if (result is null)
                return NotFound(new { Message = "Nenhuma leitura encontrada para o dispositivo especificado." });

            return Ok(result); // 200, não 202: é uma consulta, não um processamento assíncrono
        }
    }
}
