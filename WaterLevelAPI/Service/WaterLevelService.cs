using Microsoft.EntityFrameworkCore;
using WaterLevelAPI.Context;
using WaterLevelAPI.Model;

namespace WaterLevelAPI.Service
{
    public class WaterLevelService : IWaterLevelService
    {
        private readonly AppDbContext _context;

        public WaterLevelService(AppDbContext context) => _context = context;

        public Task RegisterLevelAsync(WaterLevelDTO waterLevelDTO)
        {
            if(waterLevelDTO.CurrentLevel < 0) throw new ArgumentException("Nivel inválido. O nível de água não pode ser negativo.");

            var waterLevel = new WatterLevel
            {
                DeviceId = waterLevelDTO.DeviceId,
                CurrentLevel = waterLevelDTO.CurrentLevel,
                MinLevel = waterLevelDTO.MinLevel,
                MaxLevel = waterLevelDTO.MaxLevel,
                TimesTamp = DateTime.UtcNow
            };

            _context.WaterLevels.Add(waterLevel);
            return _context.SaveChangesAsync();
        }

        public async Task<WaterLevelDTO> GetLevelAsync(int deviceId)
        {
            var entity = await _context.WaterLevels
                .AsNoTracking()
                .OrderByDescending(x => x.TimesTamp)
                .ToListAsync();

            return entity.Select(x =>  new WaterLevelDTO
            {
                DeviceId = x.DeviceId,
                CurrentLevel = x.CurrentLevel,
                MinLevel = x.MinLevel,
                MaxLevel = x.MaxLevel
            }).FirstOrDefault() ?? throw new ArgumentException("Nível de água não encontrado para o dispositivo especificado.");
        }

     
    }
}
