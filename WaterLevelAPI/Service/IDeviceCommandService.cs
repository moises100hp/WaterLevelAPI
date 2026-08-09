namespace WaterLevelAPI.Service
{
    public interface IDeviceCommandService
    {
        Task SetCommandAsync(DeviceCommandDTO deviceCommandDTO);

        // Retorna pumpOn: false quando não há registro para o dispositivo (o controller sempre responde 200).
        Task<DeviceCommandDTO> GetCommandAsync(string deviceId);
    }
}
