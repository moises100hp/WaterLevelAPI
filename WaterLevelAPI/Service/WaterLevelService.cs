using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using WaterLevelAPI.Context;
using WaterLevelAPI.Model;

namespace WaterLevelAPI.Service
{
    public class WaterLevelService : IWaterLevelService
    {
        private readonly AppDbContext _context;

        private readonly ILogger<WaterLevelService> _logger;

        public WaterLevelService(AppDbContext context)
        {
            _context = context;

        }

        public async Task RegisterLevelAsync(WaterLevelDTO waterLevelDTO)
        {
            //if(waterLevelDTO.CurrentLevel > waterLevelDTO.MaxLevel || waterLevelDTO.CurrentLevel < waterLevelDTO.MinLevel)
            //{
            //    throw new ArgumentException("Nível de água fora dos limites definidos.");
            //}

            if (waterLevelDTO.CurrentLevel < 0) throw new ArgumentException("Nivel inválido. O nível de água não pode ser negativo.");

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

        }

        public async Task<WaterLevelDTO> GetLevelAsync(string deviceId)
        {
            var entity = await _context.WaterLevels
                .AsNoTracking()
                .Where(x => x.DeviceId == deviceId)
                .OrderByDescending(x => x.TimesTamp)
                .ToListAsync();

            return entity.Select(x => new WaterLevelDTO
            {
                DeviceId = x.DeviceId,
                CurrentLevel = x.CurrentLevel,
                MinLevel = x.MinLevel,
                MaxLevel = x.MaxLevel
            }).FirstOrDefault() ?? throw new ArgumentException("Nível de água não encontrado para o dispositivo especificado.");
        }

        public async Task<PendingChangesDTO> GetStatusDevice(string deviceId)
        {
            var entity = await _context.DeviceChanges.AsNoTracking()
                .Where(x => x.DeviceId == deviceId)
                .OrderByDescending(x => x.Id)
                .ToListAsync();

            return entity.Select(x => new PendingChangesDTO
            {
                DeviceId = x.DeviceId,
                LigarDispositivo = x.StatusChanged
            }).FirstOrDefault() ?? throw new ArgumentException("Alterações pendentes não encontradas para o dispositivo especificado.");
        }

        public async Task SetStatusDevice(PendingChangesDTO changesDTO)
        {
            var device = new DeviceChange
            {
                DeviceId = changesDTO.DeviceId,
                StatusChanged = changesDTO.LigarDispositivo
            };

            _context.DeviceChanges.Add(device);
            await _context.SaveChangesAsync();
        }
    }
}
