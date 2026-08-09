using Microsoft.EntityFrameworkCore;
using WaterLevelAPI.Context;
using WaterLevelAPI.Model;

namespace WaterLevelAPI.Service
{
    public class DeviceCommandService : IDeviceCommandService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<DeviceCommandService> _logger;

        public DeviceCommandService(AppDbContext context, ILogger<DeviceCommandService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task SetCommandAsync(DeviceCommandDTO deviceCommandDTO)
        {
            if (string.IsNullOrWhiteSpace(deviceCommandDTO.DeviceId))
                throw new ArgumentException("DeviceId é obrigatório.");

            var command = await _context.DeviceCommands
                .FirstOrDefaultAsync(x => x.DeviceId == deviceCommandDTO.DeviceId);

            if (command is null)
            {
                command = new DeviceCommand
                {
                    DeviceId = deviceCommandDTO.DeviceId
                };
                _context.DeviceCommands.Add(command);
            }

            command.PumpOn = deviceCommandDTO.PumpOn;
            command.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Comando atualizado para o dispositivo {DeviceId}: PumpOn={PumpOn}",
                command.DeviceId, command.PumpOn);
        }

        public async Task<DeviceCommandDTO> GetCommandAsync(string deviceId)
        {
            var command = await _context.DeviceCommands
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.DeviceId == deviceId);

            if (command is null)
            {
                return new DeviceCommandDTO
                {
                    DeviceId = deviceId,
                    PumpOn = false
                };
            }

            return new DeviceCommandDTO
            {
                DeviceId = command.DeviceId,
                PumpOn = command.PumpOn,
                UpdatedAt = command.UpdatedAt
            };
        }
    }
}
