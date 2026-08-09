using Microsoft.AspNetCore.Mvc;
using WaterLevelAPI.Service;

namespace WaterLevelAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DeviceCommandController : ControllerBase
    {
        private readonly ILogger<DeviceCommandController> _logger;
        private readonly IDeviceCommandService _service;

        public DeviceCommandController(ILogger<DeviceCommandController> logger, IDeviceCommandService service)
        {
            _logger = logger;
            _service = service;
        }

        // POST api/devicecommand
        [HttpPost]
        public async Task<IActionResult> SetCommand(DeviceCommandDTO deviceCommandDTO)
        {
            try
            {
                await _service.SetCommandAsync(deviceCommandDTO);
                return Accepted();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gravar comando do dispositivo {DeviceId}", deviceCommandDTO.DeviceId);
                return StatusCode(500, new { Message = "Erro interno ao gravar o comando." });
            }
        }

        // GET api/devicecommand?deviceId=caixa-01
        [HttpGet]
        public async Task<IActionResult> GetCommand([FromQuery] string deviceId)
        {
            if (string.IsNullOrWhiteSpace(deviceId))
                return BadRequest(new { Message = "Informe o deviceId." });

            var result = await _service.GetCommandAsync(deviceId);

            return Ok(result);
        }
    }
}
