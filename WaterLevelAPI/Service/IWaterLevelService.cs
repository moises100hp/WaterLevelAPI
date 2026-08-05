namespace WaterLevelAPI.Service
{
    public interface IWaterLevelService
    {
        Task RegisterLevelAsync(WaterLevelDTO waterLevelDTO);

        Task<WaterLevelDTO> GetLevelAsync(string deviceId);
        Task<PendingChangesDTO> GetStatusDevice(string deviceId);
        Task SetStatusDevice(PendingChangesDTO changesDTO);
    }
}
