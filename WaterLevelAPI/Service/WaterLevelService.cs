using Microsoft.EntityFrameworkCore;
using WaterLevelAPI.Context;
using WaterLevelAPI.Model;

namespace WaterLevelAPI.Service
{
    public class WaterLevelService : IWaterLevelService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<WaterLevelService> _logger;

        public WaterLevelService(AppDbContext context, ILogger<WaterLevelService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task RegisterLevelAsync(WaterLevelDTO waterLevelDTO)
        {
            if (waterLevelDTO.CurrentLevel < 0)
                throw new ArgumentException("Nível inválido. O nível de água não pode ser negativo.");

            if (waterLevelDTO.MaxLevel <= waterLevelDTO.MinLevel)
                throw new ArgumentException("MaxLevel deve ser maior que MinLevel.");

            var waterLevel = new WatterLevel
            {
                DeviceId = waterLevelDTO.DeviceId,
                CurrentLevel = waterLevelDTO.CurrentLevel,
                MinLevel = waterLevelDTO.MinLevel,
                MaxLevel = waterLevelDTO.MaxLevel,
                TimesTamp = DateTime.UtcNow
            };

            _context.WaterLevels.Add(waterLevel);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Leitura registrada para o dispositivo {DeviceId}: {Level}",
                waterLevel.DeviceId, waterLevel.CurrentLevel);
        }

        public async Task<WaterLevelDTO?> GetLevelAsync(string deviceId)
        {
            // Filtra pelo dispositivo e traz apenas o registro mais recente,
            // tudo resolvido no banco (sem carregar a tabela inteira em memória).
            return await _context.WaterLevels
                .AsNoTracking()
                .Where(x => x.DeviceId == deviceId)
                .OrderByDescending(x => x.TimesTamp)
                .Select(x => new WaterLevelDTO
                {
                    DeviceId = x.DeviceId,
                    CurrentLevel = x.CurrentLevel,
                    MinLevel = x.MinLevel,
                    MaxLevel = x.MaxLevel
                })
                .FirstOrDefaultAsync();
        }
    }
}
