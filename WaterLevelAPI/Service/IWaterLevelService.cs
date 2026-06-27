namespace WaterLevelAPI.Service
{
    public interface IWaterLevelService
    {
        Task RegisterLevelAsync(WaterLevelDTO waterLevelDTO);

        Task<WaterLevelDTO> GetLevelAsync(int deviceId);
    }
}
