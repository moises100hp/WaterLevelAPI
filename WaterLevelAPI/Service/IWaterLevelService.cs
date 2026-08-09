namespace WaterLevelAPI.Service
{
    public interface IWaterLevelService
    {
        Task RegisterLevelAsync(WaterLevelDTO waterLevelDTO);

        // deviceId agora é string, igual ao modelo e ao DTO.
        // Retorna null quando não há leitura para o dispositivo (o controller decide o 404).
        Task<WaterLevelDTO?> GetLevelAsync(string deviceId);
    }
}
