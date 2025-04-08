using IoTDeviceManager.server.Models.Devices;

namespace IoTDeviceManager.server.CosmosDB
{
    public interface ICosmosDbService
    {
        Task<List<CDevice>> GetDevicesAsync(string userId);
        Task<CDevice?> GetDeviceAsync(string serialNumber);
        Task<dynamic> CreateDeviceAsync(CDevice device);
        Task<bool> UpdateDeviceAsync(CDevice device);
    }
}
